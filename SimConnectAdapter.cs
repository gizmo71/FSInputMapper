using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{
    enum DATA_DEFINITIONS { AUTOPILOT_DATA = 69, SPOILER_DATA }
    enum REQUESTS { AUTOPILOT_DATA = 71, MORE_SPOILER, LESS_SPOILER }

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

            simConnect.AddToDataDefinition(DATA_DEFINITIONS.SPOILER_DATA, "SPOILERS HANDLE POSITION", "percent",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DATA_DEFINITIONS.SPOILER_DATA, "SPOILERS ARMED", "Bool",
                SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.RegisterDataDefineStruct<SpoilerData>(DATA_DEFINITIONS.SPOILER_DATA);

/*TODO:
            hr = SimConnect_MapClientEventToSimEvent(hSimConnect, EVENT_ARM_SPOILER, "SPOILERS_ARM_ON");
            hr = SimConnect_MapClientEventToSimEvent(hSimConnect, EVENT_DISARM_SPOILER, "SPOILERS_ARM_OFF");

            hr = SimConnect_MapClientEventToSimEvent(hSimConnect, EVENT_MORE_SPOILER, "SPOILERS_TOGGLE");
            hr = SimConnect_MapClientEventToSimEvent(hSimConnect, EVENT_LESS_SPOILER, "SPOILERS_ARM_TOGGLE");

            hr = SimConnect_AddClientEventToNotificationGroup(hSimConnect, GROUP_SPOILERS, EVENT_MORE_SPOILER, TRUE);
            hr = SimConnect_AddClientEventToNotificationGroup(hSimConnect, GROUP_SPOILERS, EVENT_LESS_SPOILER, TRUE);
            hr = SimConnect_SetNotificationGroupPriority(hSimConnect, GROUP_SPOILERS, SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE);

            ... and all the event handling that goes along with it.
*/
        }

        private void OnRecvSimobjectData(SimConnect simConnect, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            switch ((REQUESTS)data.dwRequestID)
            {
                case REQUESTS.AUTOPILOT_DATA:
                    AutopilotData autopilotData = (AutopilotData)data.dwData[0];
                    viewModel.AutopilotAltitude = (int)autopilotData.apAltitude;
                    break;
            }
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
