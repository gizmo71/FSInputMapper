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
                        simConnect.Dispose();
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
