﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{
    enum DATA {
        // Stuff intended for struct-based multiple value requests:
        AUTOPILOT_DATA = 69, SPOILER_DATA,
        // Stuff for single value setting:
        SPOILER_HANDLE, AP_ALTITUDE }
    enum REQUEST { AUTOPILOT_DATA = 71, MORE_SPOILER, LESS_SPOILER, }
    /*TODO: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.categoryattribute?view=netcore-3.1
      Way to identify specific things?
      Would we be better to have a whole class for events and their recievers which includes an ID generator? */
    enum EVENT { NONE = 42, DISARM_SPOILER, ARM_SPOILER, MORE_SPOILER, LESS_SPOILER,
        AP_SPEED, AP_SPEED_SLOT_SET, AP_HEADING_SLOT_SET, AP_HEADING_BUG_SET, AP_ALTITUDE_SLOT_SET,
    }
    enum GROUP { SPOILERS = 13, AUTOPILOT,
        PRIORITY_STANDARD = 1900000000 }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct AutopilotData
    {
        public double apSpeed;
        public double apSpeedSlot;
        public double apHeading;
        public double apHeadingSlot;
        public double apAltitude;
        public double apAltitudeSlot;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct SpoilerData
    {
        public double spoilersHandlePosition;
        public double spoilersArmed;
    };

    class SimConnectAdapter {
        const int WM_USER_SIMCONNECT = 0x0402;

        private readonly IntPtr hWnd;
        private readonly FSIMViewModel viewModel;
        private SimConnect? simConnect;

        public SimConnectAdapter([DisallowNull] HwndSource hWndSource, FSIMViewModel viewModel)
        {
            this.hWnd = hWndSource.Handle;
            this.viewModel = viewModel;
            hWndSource.AddHook(WndProc);

            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();

            viewModel.PropertyChanged += PropertyChangeHandler;
            viewModel.TriggerBus.OnTrigger += OnTrigger; //TODO: this, later? What removes it?
        }

        private void PropertyChangeHandler(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (sender != viewModel) return;
            switch (eventArgs.PropertyName)
            {
                case nameof(viewModel.AirspeedManaged):
                    SendEvent(EVENT.AP_SPEED_SLOT_SET, viewModel.AirspeedManaged ? 2u : 1u);
//TODO: something when switching back to manual to stop the number sticking
                    break;
                case nameof(viewModel.AutopilotAirspeed) when !viewModel.AirspeedManaged:
                    SendEvent(EVENT.AP_SPEED, (uint)viewModel.AutopilotAirspeed);
                    break;
                case nameof(viewModel.HeadingManaged):
                    SendEvent(EVENT.AP_HEADING_SLOT_SET, viewModel.HeadingManaged ? 2u : 1u);
//TODO: something when switching back to manual to stop the number sticking
                    break;
                case nameof(viewModel.AutopilotHeading) when !viewModel.HeadingManaged:
                    SendEvent(EVENT.AP_HEADING_BUG_SET, (uint)viewModel.AutopilotHeading);
                    break;
                case nameof(viewModel.AltitudeManaged):
if (!viewModel.AltitudeManaged) //TODO: use triggers instead?
                    SendEvent(EVENT.AP_ALTITUDE_SLOT_SET, viewModel.AltitudeManaged ? 2u : 1u);
                    break;
                case nameof(viewModel.AutopilotAltitude) when !viewModel.AltitudeManaged:
                    SetData(DATA.AP_ALTITUDE, viewModel.AutopilotAltitude);
                    break;
            }
        }

        private void Disconnect(Exception ex)
        {
            simConnect?.Dispose();
            simConnect = null;
            viewModel.ConnectionError = ex.Message;
        }
        private void Tick(object? sender, EventArgs e)
        {
            if (simConnect != null) return;
            try
            {
                Connect();
                viewModel.ConnectionError = null;
            }
            catch (COMException ex)
            {
                Disconnect(ex);
            }
        }

        private void Connect()
        {
            simConnect = new SimConnect("Gizmo's FSInputMapper", hWnd, WM_USER_SIMCONNECT, null, 0);
            simConnect.OnRecvOpen += OnRecvOpen;
            simConnect.OnRecvQuit += OnRecvQuit;
            simConnect.OnRecvSimobjectData += OnRecvSimobjectData;
            simConnect.OnRecvEvent += OnRecvEvent;
        }

        private void OnRecvQuit(SimConnect simConnect, SIMCONNECT_RECV data)
        {
            Disconnect(new Exception("Sim exited"));
        }

        private void OnRecvOpen(SimConnect simConnect, SIMCONNECT_RECV_OPEN data)
        {
            // Autopilot things we receive.

            // Correct, but not writable.
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT AIRSPEED HOLD VAR", "knots",
                SIMCONNECT_DATATYPE.FLOAT64, 2.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT SPEED SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            // Correct, but not writable.
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT HEADING LOCK DIR", "degrees",
                SIMCONNECT_DATATYPE.FLOAT64, 2.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT HEADING SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            // In selected mode, this is correct (but not writable).
            // In managed mode, it shows what the autopilot is really doing (which may be modified by constraints).
            // Have not yet found where the displayed panel value is (may not be available via SimConnect).
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT ALTITUDE LOCK VAR", "feet",
                SIMCONNECT_DATATYPE.FLOAT64, 50f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT ALTITUDE SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            simConnect.RegisterDataDefineStruct<AutopilotData>(DATA.AUTOPILOT_DATA);
            simConnect.RequestDataOnSimObject(REQUEST.AUTOPILOT_DATA, DATA.AUTOPILOT_DATA,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);

            // Autopilot things we set.

            simConnect.AddToDataDefinition(DATA.AP_ALTITUDE, "AUTOPILOT ALTITUDE LOCK VAR", "feet",
                SIMCONNECT_DATATYPE.FLOAT64, 50f, SimConnect.SIMCONNECT_UNUSED);

            // Autopilot things we send.

            simConnect.MapClientEventToSimEvent(EVENT.AP_SPEED_SLOT_SET, "SPEED_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_SPEED, "AP_SPD_VAR_SET"); // "AP REFERENCE AIRSPEED IN KNOTS" in GUI = "INPUT.KEY_AP_SPD_VAR_SET"

            simConnect.MapClientEventToSimEvent(EVENT.AP_HEADING_SLOT_SET, "HEADING_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_HEADING_BUG_SET, "HEADING_BUG_SET");

            simConnect.MapClientEventToSimEvent(EVENT.AP_ALTITUDE_SLOT_SET, "ALTITUDE_SLOT_INDEX_SET");

            // Spoilers

            simConnect.AddToDataDefinition(DATA.SPOILER_HANDLE, "SPOILERS HANDLE POSITION", "percent",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            simConnect.AddToDataDefinition(DATA.SPOILER_DATA, "SPOILERS HANDLE POSITION", "percent",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.SPOILER_DATA, "SPOILERS ARMED", "Bool",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.RegisterDataDefineStruct<SpoilerData>(DATA.SPOILER_DATA);

            // Spoilers: things we recieve.

            simConnect.MapClientEventToSimEvent(EVENT.MORE_SPOILER, "SPOILERS_TOGGLE");
            simConnect.MapClientEventToSimEvent(EVENT.LESS_SPOILER, "SPOILERS_ARM_TOGGLE");

            simConnect.AddClientEventToNotificationGroup(GROUP.SPOILERS, EVENT.MORE_SPOILER, true);
            simConnect.AddClientEventToNotificationGroup(GROUP.SPOILERS, EVENT.LESS_SPOILER, true);

            simConnect.SetNotificationGroupPriority(GROUP.SPOILERS, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE);

            // Spoilers: things we send (we've artificially partitioned these as I couldn't avoid hearing our own event).

            simConnect.MapClientEventToSimEvent(EVENT.ARM_SPOILER, "SPOILERS_ARM_ON");
            simConnect.MapClientEventToSimEvent(EVENT.DISARM_SPOILER, "SPOILERS_ARM_OFF");

            simConnect.SetNotificationGroupPriority(GROUP.AUTOPILOT, SimConnect.SIMCONNECT_GROUP_PRIORITY_STANDARD);
        }

        private void OnRecvEvent(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            switch ((EVENT)data.uEventID) {
                case EVENT.LESS_SPOILER:
                    simConnect.RequestDataOnSimObject(REQUEST.LESS_SPOILER, DATA.SPOILER_DATA,
                        SimConnect.SIMCONNECT_OBJECT_ID_USER,
                        SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                    break;
                case EVENT.MORE_SPOILER:
                    simConnect.RequestDataOnSimObject(REQUEST.MORE_SPOILER, DATA.SPOILER_DATA,
                        SimConnect.SIMCONNECT_OBJECT_ID_USER,
                        SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                    break;
            }
        }

        private void OnRecvSimobjectData(SimConnect simConnect, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            switch ((REQUEST)data.dwRequestID)
            {
                case REQUEST.AUTOPILOT_DATA:
                    AutopilotData autopilotData = (AutopilotData)data.dwData[0];
                    viewModel.AirspeedManaged = autopilotData.apSpeedSlot == 2;
                    viewModel.AutopilotAirspeed = (int)autopilotData.apSpeed;
                    viewModel.HeadingManaged = autopilotData.apHeadingSlot == 2;
                    viewModel.AutopilotHeading = (int)autopilotData.apHeading;
                    viewModel.AltitudeManaged = autopilotData.apAltitudeSlot == 2;
                    viewModel.AutopilotAltitude = (int)autopilotData.apAltitude;
                    break;
                case REQUEST.MORE_SPOILER:
                    SpoilerData spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersArmed != 0)
                        SendEvent(EVENT.DISARM_SPOILER);
                    else
                        SetData(DATA.SPOILER_HANDLE, Math.Min(spoilerData.spoilersHandlePosition + 25.0, 100.0));
                    break;
                case REQUEST.LESS_SPOILER:
                    spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersHandlePosition > 0.0)
                        SetData(DATA.SPOILER_HANDLE, Math.Max(spoilerData.spoilersHandlePosition - 25.0, 0.0));
                    else if (spoilerData.spoilersArmed == 0)
                        SendEvent(EVENT.ARM_SPOILER);
                    break;
            }
        }

        private void SendEvent(EVENT eventToSend, uint data = 0) {
            simConnect?.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, data, GROUP.PRIORITY_STANDARD, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        private void SetData(DATA data, object value) {
            simConnect?.SetDataOnSimObject(data, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, value);
        }

        private void OnTrigger(object? sender, FSIMTrigger e)
        {
            SendEvent(EVENT.AP_ALTITUDE_SLOT_SET, 2u);
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled) {
            if (iMsg == WM_USER_SIMCONNECT)
            {
                try
                {
                    simConnect?.ReceiveMessage();
                } catch (Exception ex) {
                    Disconnect(ex);
                }
            }

            return IntPtr.Zero;
        }
    }

}
