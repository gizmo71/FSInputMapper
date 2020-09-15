using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{
    enum DATA_DEFINITIONS { AUTOPILOT_DATA = 69, SPOILER_DATA, SPOILER_HANDLE, }
    enum REQUESTS { AUTOPILOT_DATA = 71, MORE_SPOILER, LESS_SPOILER, }
    enum EVENTS { NONE = 42, DISARM_SPOILER, ARM_SPOILER, MORE_SPOILER, LESS_SPOILER, }
    enum GROUPS { SPOILERS = 13, }

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
            // Autopilot

            simConnect.AddToDataDefinition(DATA_DEFINITIONS.AUTOPILOT_DATA, "AUTOPILOT AIRSPEED HOLD VAR", "knots",
                SIMCONNECT_DATATYPE.FLOAT64, 2.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA_DEFINITIONS.AUTOPILOT_DATA, "AUTOPILOT SPEED SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            simConnect.AddToDataDefinition(DATA_DEFINITIONS.AUTOPILOT_DATA, "AUTOPILOT HEADING LOCK DIR", "degrees",
                SIMCONNECT_DATATYPE.FLOAT64, 2.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA_DEFINITIONS.AUTOPILOT_DATA, "AUTOPILOT HEADING SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            simConnect.AddToDataDefinition(DATA_DEFINITIONS.AUTOPILOT_DATA, "AUTOPILOT ALTITUDE LOCK VAR", "feet",
                SIMCONNECT_DATATYPE.FLOAT64, 100f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA_DEFINITIONS.AUTOPILOT_DATA, "AUTOPILOT ALTITUDE SLOT INDEX", "number",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            //TODO: add vertical speed, "AUTOPILOT VERTICAL HOLD VAR" in "feet/minute", and maybe "AUTOPILOT VERTICAL HOLD"

            simConnect.RegisterDataDefineStruct<AutopilotData>(DATA_DEFINITIONS.AUTOPILOT_DATA);
            simConnect.RequestDataOnSimObject(REQUESTS.AUTOPILOT_DATA, DATA_DEFINITIONS.AUTOPILOT_DATA,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);

            // Spoilers

            simConnect.AddToDataDefinition(DATA_DEFINITIONS.SPOILER_HANDLE, "SPOILERS HANDLE POSITION", "percent",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

            simConnect.AddToDataDefinition(DATA_DEFINITIONS.SPOILER_DATA, "SPOILERS HANDLE POSITION", "percent",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA_DEFINITIONS.SPOILER_DATA, "SPOILERS ARMED", "Bool",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.RegisterDataDefineStruct<SpoilerData>(DATA_DEFINITIONS.SPOILER_DATA);

            simConnect.MapClientEventToSimEvent(EVENTS.MORE_SPOILER, "SPOILERS_TOGGLE");
            simConnect.MapClientEventToSimEvent(EVENTS.LESS_SPOILER, "SPOILERS_ARM_TOGGLE");

            simConnect.MapClientEventToSimEvent(EVENTS.ARM_SPOILER, "SPOILERS_ARM_ON");
            simConnect.MapClientEventToSimEvent(EVENTS.DISARM_SPOILER, "SPOILERS_ARM_OFF");

            simConnect.AddClientEventToNotificationGroup(GROUPS.SPOILERS, EVENTS.MORE_SPOILER, true);
            simConnect.AddClientEventToNotificationGroup(GROUPS.SPOILERS, EVENTS.LESS_SPOILER, true);
            simConnect.SetNotificationGroupPriority(GROUPS.SPOILERS, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE);
        }

        private void OnRecvEvent(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            switch ((EVENTS)data.uEventID) {
                case EVENTS.LESS_SPOILER:
                    simConnect.RequestDataOnSimObject(REQUESTS.LESS_SPOILER, DATA_DEFINITIONS.SPOILER_DATA,
                        SimConnect.SIMCONNECT_OBJECT_ID_USER,
                        SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                    break;
                case EVENTS.MORE_SPOILER:
                    simConnect.RequestDataOnSimObject(REQUESTS.MORE_SPOILER, DATA_DEFINITIONS.SPOILER_DATA,
                        SimConnect.SIMCONNECT_OBJECT_ID_USER,
                        SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                    break;
            }
        }

        private void OnRecvSimobjectData(SimConnect simConnect, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            switch ((REQUESTS)data.dwRequestID)
            {
                case REQUESTS.AUTOPILOT_DATA:
                    AutopilotData autopilotData = (AutopilotData)data.dwData[0];
                    viewModel.AutopilotAltitude = (int)autopilotData.apAltitude;
                    viewModel.AltitudeManaged = autopilotData.apAltitudeSlot == 2;
                    break;
                case REQUESTS.MORE_SPOILER:
                    SpoilerData spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersArmed != 0)
                        SendEvent(GROUPS.SPOILERS, EVENTS.DISARM_SPOILER);
                    else
                        SetSpoilerHandlePosition(Math.Min(spoilerData.spoilersHandlePosition + 25.0, 100.0));
                    break;
                case REQUESTS.LESS_SPOILER:
                    spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersHandlePosition > 0.0)
                        SetSpoilerHandlePosition(Math.Max(spoilerData.spoilersHandlePosition - 25.0, 0.0));
                    else if (spoilerData.spoilersArmed == 0)
                        SendEvent(GROUPS.SPOILERS, EVENTS.ARM_SPOILER);
                    break;
            }
        }

        private void SendEvent(GROUPS group, EVENTS eventToSend) {
            simConnect?.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, 0, group, 0);
        }

        private void SetSpoilerHandlePosition(double percent) {
            simConnect?.SetDataOnSimObject(DATA_DEFINITIONS.SPOILER_HANDLE, SimConnect.SIMCONNECT_OBJECT_ID_USER, 0, percent);
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
