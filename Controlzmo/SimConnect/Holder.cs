using Controlzmo;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    [Component]
    public class SimConnectHolder
    {
        public SimConnect? SimConnect { get; internal set; }
    }
}
