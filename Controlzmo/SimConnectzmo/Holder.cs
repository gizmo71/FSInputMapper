using System;
using Controlzmo;

namespace SimConnectzmo
{
    [Component]
    public class SimConnectHolder : IDisposable
    {
        private ExtendedSimConnect? simConnect;
        public ExtendedSimConnect? SimConnect
        {
            get { return simConnect; }
            internal set
            {
                if (simConnect != value)
                {
                    if (simConnect != null)
                    {
                        // This causes an ucatchable System.AccessViolationException: simConnect.Dispose();
                    }
                    simConnect = value;
                }
            }
        }

        public void Dispose()
        {
            SimConnect = null;
        }
    }
}
