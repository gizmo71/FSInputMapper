using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{
    class SimConnectAdapter {
        const int WM_USER_SIMCONNECT = 0x0402;

        private readonly IntPtr hWnd;
        private SimConnect? simConnect;

        public SimConnectAdapter([DisallowNull] HwndSource hWndSource)
        {
            this.hWnd = hWndSource.Handle;
            hWndSource.AddHook(WndProc);
        }

        public Boolean IsConnected()
        {
            return simConnect != null;
        }

        public void Disconnect()
        {
            simConnect?.Dispose();
            simConnect = null;
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
