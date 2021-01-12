using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct StrobeLightData
    {
        // "Auto" comes back as on. :-(
        [SimVar("LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public int strobeSwitch;
        [SimVar("LIGHT STROBE ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public int strobeState;
        [SimVar("LIGHT POTENTIOMETER:24", "Number", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public int lPot24;
    };

    [Component]
    public class StrobeLightListener : DataListener<StrobeLightData>, IRequestDataOnOpen
    {
        private readonly IHubContext<LightHub, ILightHub> hub;
        private readonly ILogger<StrobeLightData> _logging;

        public StrobeLightListener(IHubContext<LightHub, ILightHub> hub, ILogger<StrobeLightData> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(ExtendedSimConnect simConnect, StrobeLightData data)
        {
            var strobesOn = data.strobeSwitch == 1;
            var strobesAuto = data.lPot24 == 0;
            _logging.LogDebug($"Strobes on? {strobesOn} Strobes auto? {strobesAuto}");
            hub.Clients.All.SetStrobeLights(strobesAuto ? null : strobesOn);
        }
    }

    [Component]
    public class SetStrobeLightsEvent : IEvent
    {
        public string SimEvent() { return "STROBES_SET"; }
    }
}
