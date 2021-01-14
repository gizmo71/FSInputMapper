using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TaxiTurnoffLightData
    {
        [SimVar("LIGHT TAXI", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseSwitch;
        [SimVar("LIGHT TAXI:2", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseSwitch2; // runway turn off (left?)
        [SimVar("LIGHT TAXI:3", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseSwitch3; // runway turn off (right?)
        [SimVar("LIGHT TAXI ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseState;
    };

    [Component]
    public class TaxiTurnoffLightListener : DataListener<TaxiTurnoffLightData>, IRequestDataOnOpen
    {
        private readonly IHubContext<LightHub, ILightHub> hub;
        private readonly ILogger<TaxiTurnoffLightListener> _logging;

        public TaxiTurnoffLightListener(IHubContext<LightHub, ILightHub> hub, ILogger<TaxiTurnoffLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(ExtendedSimConnect simConnect, TaxiTurnoffLightData data)
        {
            _logging.LogDebug($"Nose light state {data.noseState}, switch 2 {data.noseSwitch2}, switch 3 {data.noseSwitch3}");
            hub.Clients.All.SetFromSim("TaxiTurnoff", data.noseSwitch == 1);
        }
    }

    [Component]
    public class SetTaxiTurnoffLightsEvent : IEvent
    {
        public string SimEvent() { return "TAXI_LIGHTS_SET"; }
    }
}
