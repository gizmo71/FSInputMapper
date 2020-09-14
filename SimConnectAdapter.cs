using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{
    enum DATA_DEFINITIONS { AUTOPILOT_DATA = 69, }
    enum REQUESTS { AUTOPILOT_DATA = 71, }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ApDataStruct
    {
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public double apAltitude;
    };

    class SimConnectAdapter {
        const int WM_USER_SIMCONNECT = 0x0402;

        private readonly IntPtr hWnd;
        private readonly FSIMViewModel viewModel;
        private SimConnect? simConnect;

        ApDataStruct ApData;

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
            simConnect.AddToDataDefinition(DATA_DEFINITIONS.AUTOPILOT_DATA, "AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 2.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.RegisterDataDefineStruct<ApDataStruct>(DATA_DEFINITIONS.AUTOPILOT_DATA);
            simConnect.RequestDataOnSimObject(REQUESTS.AUTOPILOT_DATA, DATA_DEFINITIONS.AUTOPILOT_DATA, SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
        }

        private void OnRecvSimobjectData(SimConnect simConnect, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            ApDataStruct apData = (ApDataStruct)data.dwData[0];
            viewModel.Altitude = (int)apData.apAltitude;
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
