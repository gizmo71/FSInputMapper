using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{
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

        public void Disconnect()
        {
            simConnect?.Dispose();
            simConnect = null;
            viewModel.ConnectionError = "Disconnected";
        }
        private void Tick(object? sender, EventArgs e)
        {
            if (IsConnected()) return;
            try {
                simConnect = new SimConnect("Gizmo's FSInputMapper", hWnd, WM_USER_SIMCONNECT, null, 0);
                viewModel.ConnectionError = null;
            }
            catch (COMException ex)
            {
                simConnect = null;
                viewModel.ConnectionError = ex.Message;
                //viewModel.Altitude = (Int32)DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000;
            }
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled)
        {
            if (iMsg == WM_USER_SIMCONNECT)
            {
                try
                {
                    simConnect?.ReceiveMessage();
                } catch {
                    Disconnect();
                }
            }

            return IntPtr.Zero;
        }
    }

}
