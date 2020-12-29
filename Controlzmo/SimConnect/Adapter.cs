using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using System.ComponentModel;

// Based on http://www.prepar3d.com/forum/viewtopic.php?p=44893&sid=3b0bd3aae23dc7b9cb0de012bab9daec#p44893
namespace SimConnectzmo
{
    public class Adapter
    {
        private const uint WM_USER_SIMCONNECT = 0x0402;
        private IntPtr hWnd = IntPtr.Zero;
        public AutoResetEvent MessageSignal = new AutoResetEvent(false);
        BackgroundWorker? bw;
        string status = "Uninitialised";

        public string TestIt()
        {
            if (bw != null) return "Already connected " + status;
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Donkey;
            bw.RunWorkerAsync();
            return "Attempting to connect in background " + status;
        }

        private void Donkey(object? sender, DoWorkEventArgs args)
        {
            try
            {
                var scinfo = new SimConnect("Controlzmo", hWnd, WM_USER_SIMCONNECT, MessageSignal, 0u);
                while (MessageSignal.WaitOne(1))
                {
                    object locker = new(); // Do we need this?
                    lock (locker)
                    {
                        scinfo.ReceiveMessage();
                    }
                }
            }
            catch (Exception e)
            {
                status = e.Message;
                bw = null;
            }
        }
    }
}
