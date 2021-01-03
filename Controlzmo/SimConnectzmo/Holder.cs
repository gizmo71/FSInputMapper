using Controlzmo;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    [Component]
    public class SimConnectHolder
    {
        public ExtendedSimConnect? SimConnect { get; internal set; }
    }
}
