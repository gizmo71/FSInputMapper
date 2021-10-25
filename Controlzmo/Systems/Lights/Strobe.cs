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
    public class StrobeLightSystem : ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly JetBridgeSender sender;

        public StrobeLightSystem(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            sender = sp.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "lightsStrobe";

        public void SetInSim(ExtendedSimConnect simConnect, string? position)
        {
            var auto = position == "auto" ? 1 : 0;
            var set = position != "off" ? 1 : 0;
            var value = position switch { "on" => 0, "auto" => 1, "off" => 2, _ => throw new ArgumentException($"Unknown strobe position {position}") };
            sender.Execute(simConnect, $"{auto} (>L:STROBE_0_Auto) {set} 0 r (>K:2:STROBES_SET) {value} (>L:LIGHTING_STROBE_0)");
        }
    }
}
