using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{
    enum DATA { AUTOPILOT_DATA = 69, SPOILER_DATA, SPOILER_HANDLE, }
    enum REQUEST { AUTOPILOT_DATA = 71, MORE_SPOILER, LESS_SPOILER, }
    /*TODO: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.categoryattribute?view=netcore-3.1
      Way to identify specific things?
      Would we be better to have a whole class for events and their recievers which includes an ID generator? */
    enum EVENT { NONE = 42, DISARM_SPOILER, ARM_SPOILER, MORE_SPOILER, LESS_SPOILER,
        AP_HEADING_SLOT_SET, AP_SPEED_SLOT_SET, AP_ALTITUDE_SLOT_SET,
    }
    enum GROUP { SPOILERS = 13, AUTOPILOT, }

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
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Start();

            viewModel.PropertyChanged += new PropertyChangedEventHandler(ViewModelPropertyChangeHandler);
        }

        private void ViewModelPropertyChangeHandler(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(viewModel.AltitudeManaged))
            {
                SendEvent(GROUP.AUTOPILOT, EVENT.AP_ALTITUDE_SLOT_SET, viewModel.AltitudeManaged ? 2u : 1u);
            }
        }

        public Boolean IsConnected()
        {
            return simConnect != null;
        }

        private void Disconnect(Exception ex)
        {
            simConnect?.Dispose();
            simConnect = null;
            viewModel.ConnectionError = ex.Message;
        }
        private void Tick(object? sender, EventArgs e)
        {
            if (IsConnected()) return;
            try
            {
                ConnectAndInitialise();
                viewModel.ConnectionError = null;
            }
            catch (COMException ex)
            {
                Disconnect(ex);
            }
        }

        private void ConnectAndInitialise()
        {
            simConnect = new SimConnect("Gizmo's FSInputMapper", hWnd, WM_USER_SIMCONNECT, null, 0);
            simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(OnRecvOpen);
            simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(OnRecvQuit);
            simConnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(OnRecvSimobjectData);
            simConnect.OnRecvEvent += new SimConnect.RecvEventEventHandler(OnRecvEvent);
        }

        private void OnRecvQuit(SimConnect simConnect, SIMCONNECT_RECV data)
        {
            Disconnect(new Exception("Sim exited"));
        }

        private void OnRecvOpen(SimConnect simConnect, SIMCONNECT_RECV_OPEN data)
        {
            // Autopilot things we receive.

            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT AIRSPEED HOLD VAR", "knots",
                SIMCONNECT_DATATYPE.FLOAT64, 2.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT SPEED SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT HEADING LOCK DIR", "degrees",
                SIMCONNECT_DATATYPE.FLOAT64, 2.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT HEADING SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT ALTITUDE LOCK VAR", "feet",
                SIMCONNECT_DATATYPE.FLOAT64, 50f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT ALTITUDE SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            //TODO: add vertical speed, "AUTOPILOT VERTICAL HOLD VAR" in "feet/minute", and maybe "AUTOPILOT VERTICAL HOLD"

            simConnect.RegisterDataDefineStruct<AutopilotData>(DATA.AUTOPILOT_DATA);
            simConnect.RequestDataOnSimObject(REQUEST.AUTOPILOT_DATA, DATA.AUTOPILOT_DATA,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);

            // Autopilot things we send.

            simConnect.MapClientEventToSimEvent(EVENT.AP_SPEED_SLOT_SET, "SPEED_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_HEADING_SLOT_SET, "HEADING_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_ALTITUDE_SLOT_SET, "ALTITUDE_SLOT_INDEX_SET");
            // https://forums.flightsimulator.com/t/airbus-neo-is-there-a-binding-to-switch-between-managed-and-selected-modes/244977/26?u=dgymer

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

            // Spoilers: things we send (we've artificially partitioned these as I could avoid hearing our own event).

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
                    viewModel.AutopilotAltitude = (int)autopilotData.apAltitude;
                    viewModel.AltitudeManaged = autopilotData.apAltitudeSlot == 2;
                    break;
                case REQUEST.MORE_SPOILER:
                    SpoilerData spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersArmed != 0)
                        SendEvent(GROUP.SPOILERS, EVENT.DISARM_SPOILER);
                    else
                        SetSpoilerHandlePosition(Math.Min(spoilerData.spoilersHandlePosition + 25.0, 100.0));
                    break;
                case REQUEST.LESS_SPOILER:
                    spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersHandlePosition > 0.0)
                        SetSpoilerHandlePosition(Math.Max(spoilerData.spoilersHandlePosition - 25.0, 0.0));
                    else if (spoilerData.spoilersArmed == 0)
                        SendEvent(GROUP.SPOILERS, EVENT.ARM_SPOILER);
                    break;
            }
        }

        private void SendEvent(GROUP group, EVENT eventToSend, uint data = 0) {
            simConnect?.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, data, group, 0);
        }

        private void SetSpoilerHandlePosition(double percent) {
            simConnect?.SetDataOnSimObject(DATA.SPOILER_HANDLE, SimConnect.SIMCONNECT_OBJECT_ID_USER, 0, percent);
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled)
        {
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
