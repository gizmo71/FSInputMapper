using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct StrobeLightStateData
    {
        [SimVar("LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public int strobeSwitch; // 0 (off) or 1 (on or auto)
        [SimVar("LIGHT POTENTIOMETER:24", "percent", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public int lPot24; // 0 (auto on ground) or 100 (not auto or in air)
    };

    [Component]
    public class StrobeLightSystem : DataListener<StrobeLightStateData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger _logging;
        private readonly JetBridgeSender sender;

        public StrobeLightSystem(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<StrobeLightSystem> _logging, JetBridgeSender sender)
        {
            this.hub = hub;
            this._logging = _logging;
            this.sender = sender;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, StrobeLightStateData data)
        {
            var switchOn = data.strobeSwitch == 1;
            var inAuto = data.lPot24 == 0 && switchOn;
            _logging.LogDebug($"Strobes on? {switchOn} auto? {inAuto} (from pot24 {data.lPot24} switch {data.strobeSwitch})");
            hub.Clients.All.SetFromSim(GetId(), inAuto ? "auto" : (switchOn ? "on" : "off"));
            if (inAuto)
                hub.Clients.All.SetFromSim(GetId(), null);
        }

        public string GetId() => "lightsStrobe";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            if (value == "auto")
            {
                //TODO: find the LVar and set it appropriately
                hub.Clients.All.Speak("I think the Strobe auto position is Borked");
                return;
            }
            var set = value != "off" ? 1 : 0;
            sender.Execute(simConnect, $" {set} (>K:STROBES_SET)");
        }
    }
}
