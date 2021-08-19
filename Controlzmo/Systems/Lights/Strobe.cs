using System;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class StrobeLightSystem : LVar, IOnSimConnection, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly JetBridgeSender sender;

        public StrobeLightSystem(IServiceProvider sp) : base(sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            sender = sp.GetRequiredService<JetBridgeSender>();
        }

        protected override string LVarName() => "LIGHTING_STROBE_0";
        protected override int Milliseconds() => 1000;
        protected override double Default() => -1;

        public void OnConnection(ExtendedSimConnect simConnect)
        {
            //TODO: remove this hack when we find a better way to trigger on connection.
            Request(simConnect, 0);
            Request(simConnect);
        }

        protected override double? Value { set => hub.Clients.All.SetFromSim(GetId(), (base.Value = value) switch { 0 => "on", 2 => "off", 1 => "auto", _ => null }); }

        public string GetId() => "lightsStrobe";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            var auto = value == "auto" ? 1 : 0;
            var set = value == "off" ? 0 : 1;
            sender.Execute(simConnect, $"{auto} (>L:STROBE_0_Auto) {set} (>K:STROBES_SET)");
        }
    }
}
