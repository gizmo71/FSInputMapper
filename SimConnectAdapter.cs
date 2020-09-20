using System;
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
    enum EVENT { NONE = 42,
        DISARM_SPOILER, ARM_SPOILER, MORE_SPOILER, LESS_SPOILER,
        AP_SPEED_SLOT_SET, AP_SPD_UP, AP_SPD_DOWN,
        AP_HEADING_SLOT_SET, AP_HDG_RIGHT, AP_HDG_LEFT, AP_HEADING_BUG_SET,
        AP_ALTITUDE_SLOT_SET, AP_ALT_UP, AP_ALT_DOWN,
        AP_TOGGLE_LOC, AP_TOGGLE_APPR,
    }
    enum GROUP { SPOILERS = 13, AUTOPILOT,
        PRIORITY_STANDARD = 1900000000 }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct AutopilotData
    {
        public double speedKnots; // Real range 100 -399 knots (Mach 0.10-0.99).
        public double speedSlot;
        public double heading; // Real range 000-359 (not 360!)
        public double headingSlot;
        public double altitude; // Real range 100-49000
        public double altitudeSlot;
        public double loc;
        public double appr;
        //TODO: V/S mode
        //TODO: set V/S to 0 on push, and engage selected V/S on pull; after 0ing, subsequent turns are actioned immediately
        //TODO: V/S selector; real range ±6000ft/min in steps of 100, or ±9.9º in steps of 0.1º
        //TODO: SPD/MCH buton; is it implemented yet?
        //TODO: HDG-V/S TRK-FPA toggle, when it's implemented
        //TODO: AP1 - AP2 - A/THR
        //TODO: EXPED button, when it's implemented
        //TODO: metric alt - does this work in MSFS?
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct SpoilerData
    {
        public double spoilersHandlePosition;
        public double spoilersArmed;
    };

    class SimConnectAdapter {
        private const int WM_USER_SIMCONNECT = 0x0402;

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

            viewModel.TriggerBus.OnTrigger += OnTrigger; //TODO: this, later? What removes it?
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
                SIMCONNECT_DATATYPE.FLOAT64, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT SPEED SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            // Correct for selected, but not writable. When the user is pre-selecting, remains on the managed number.
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT HEADING LOCK DIR", "degrees",
                SIMCONNECT_DATATYPE.FLOAT64, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT HEADING SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            // In selected mode, this is correct (but not writable).
            // In managed mode, it shows what the autopilot is really doing (which may be modified by constraints).
            // Have not yet found where the displayed panel value is (may not be available via SimConnect).
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT ALTITUDE LOCK VAR", "feet",
                SIMCONNECT_DATATYPE.FLOAT64, 50f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT ALTITUDE SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT NAV1 LOCK", "Bool",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA.AUTOPILOT_DATA, "AUTOPILOT APPROACH HOLD", "Bool",
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
            simConnect.MapClientEventToSimEvent(EVENT.AP_SPD_UP, "AP_SPD_VAR_INC");
            simConnect.MapClientEventToSimEvent(EVENT.AP_SPD_DOWN, "AP_SPD_VAR_DEC");

            simConnect.MapClientEventToSimEvent(EVENT.AP_HEADING_SLOT_SET, "HEADING_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_HDG_RIGHT, "HEADING_BUG_INC");
            simConnect.MapClientEventToSimEvent(EVENT.AP_HDG_LEFT, "HEADING_BUG_DEC");
            simConnect.MapClientEventToSimEvent(EVENT.AP_HEADING_BUG_SET, "HEADING_BUG_SET");

            simConnect.MapClientEventToSimEvent(EVENT.AP_ALTITUDE_SLOT_SET, "ALTITUDE_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_ALT_UP, "AP_ALT_VAR_INC");
            simConnect.MapClientEventToSimEvent(EVENT.AP_ALT_DOWN, "AP_ALT_VAR_DEC");

            simConnect.MapClientEventToSimEvent(EVENT.AP_TOGGLE_LOC, "AP_LOC_HOLD");
            simConnect.MapClientEventToSimEvent(EVENT.AP_TOGGLE_APPR, "AP_APR_HOLD");

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
                    viewModel.AirspeedManaged = autopilotData.speedSlot == 2;
                    viewModel.AutopilotAirspeed = (int)autopilotData.speedKnots;
                    viewModel.HeadingManaged = autopilotData.headingSlot == 2;
                    viewModel.AutopilotHeading = (int)autopilotData.heading;
                    viewModel.AltitudeManaged = autopilotData.altitudeSlot == 2;
                    viewModel.AutopilotAltitude = (int)autopilotData.altitude;
                    viewModel.AutopilotLoc = autopilotData.loc == 1;
                    viewModel.AutopilotAppr = autopilotData.appr == 1;
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

        private void OnTrigger(object? sender, FSIMTrigger e)
        {
            switch (e.What)
            {
                case FSIMTrigger.SPD_MAN:
                    //TODO: send something to unset the locked selected value
                    SendEvent(EVENT.AP_SPEED_SLOT_SET, 2u);
                    break;
                case FSIMTrigger.SPD_SEL:
                    SendEvent(EVENT.AP_SPEED_SLOT_SET, 1u);
                    break;
                case FSIMTrigger.SPD_1_FASTER:
                    SendEvent(EVENT.AP_SPD_UP, fast: true);
                    break;
                case FSIMTrigger.SPD_10_FASTER:
                    SendEvent(EVENT.AP_SPD_UP, slow: true);
                    break;
                case FSIMTrigger.SPD_1_SLOWER:
                    SendEvent(EVENT.AP_SPD_DOWN, fast: true);
                    break;
                case FSIMTrigger.SPD_10_SLOWER:
                    SendEvent(EVENT.AP_SPD_DOWN, slow: true);
                    break;
                case FSIMTrigger.HDG_MAN:
                    SendEvent(EVENT.AP_HEADING_SLOT_SET, 2u);
                    break;
                case FSIMTrigger.HDG_SEL:
                    //TODO: if no preselection, set heading bug to current heading?
                    SendEvent(EVENT.AP_HEADING_SLOT_SET, 1u);
                    break;
                case FSIMTrigger.HDG_RIGHT_1:
                    SendEvent(EVENT.AP_HDG_RIGHT, fast: true);
                    break;
                case FSIMTrigger.HDG_RIGHT_10:
                    SendEvent(EVENT.AP_HDG_RIGHT, slow: true);
                    break;
                case FSIMTrigger.HDG_LEFT_1:
                    SendEvent(EVENT.AP_HDG_LEFT, fast: true);
                    break;
                case FSIMTrigger.HDG_LEFT_10:
                    SendEvent(EVENT.AP_HDG_LEFT, slow: true);
                    break;
                case FSIMTrigger.ALT_MAN:
                    SendEvent(EVENT.AP_ALTITUDE_SLOT_SET, 2u);
                    break;
                case FSIMTrigger.ALT_SEL:
                    SendEvent(EVENT.AP_ALTITUDE_SLOT_SET, 1u);
                    break;
                case FSIMTrigger.ALT_UP_1000:
                    SendEvent(EVENT.AP_ALT_UP, 1000u);
                    break;
                case FSIMTrigger.ALT_UP_100:
                    SendEvent(EVENT.AP_ALT_UP, 100u);
                    break;
                case FSIMTrigger.ALT_DOWN_1000:
                    SendEvent(EVENT.AP_ALT_DOWN, 1000u);
                    break;
                case FSIMTrigger.ALT_DOWN_100:
                    SendEvent(EVENT.AP_ALT_DOWN, 100u);
                    break;
                case FSIMTrigger.TOGGLE_LOC_MODE:
                    SendEvent(EVENT.AP_TOGGLE_LOC);
                    break;
                case FSIMTrigger.TOGGLE_APPR_MODE:
                    SendEvent(EVENT.AP_TOGGLE_APPR);
                    break;
            }
        }

        private void SetData(DATA data, object value)
        {
            simConnect?.SetDataOnSimObject(data, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, value);
        }

        private void SendEvent(EVENT eventToSend, uint data = 0u, bool slow = false, bool fast = false)
        {
            SIMCONNECT_EVENT_FLAG flags = SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY;
            if (slow) flags |= SIMCONNECT_EVENT_FLAG.SLOW_REPEAT_TIMER;
            if (fast) flags |= SIMCONNECT_EVENT_FLAG.FAST_REPEAT_TIMER;
            simConnect?.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, data, GROUP.PRIORITY_STANDARD, flags);
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
