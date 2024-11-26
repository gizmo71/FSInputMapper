using System;
using Controlzmo.Hubs;
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
            if (simConnect.IsFBW) {
                var auto = position == "auto" ? 1 : 0;
                var set = position != "off" ? 1 : 0;
                var value = position switch { "on" => 0, "auto" => 1, "off" => 2, _ => throw new ArgumentException($"Unknown strobe position {position}") };
                sender.Execute(simConnect, $"{auto} (>L:STROBE_0_Auto) {set} 0 r (>K:2:STROBES_SET) {value} (>L:LIGHTING_STROBE_0)");
            }
            else if (simConnect.IsFenix)
            {
                var code = position switch {  "on" => 2, "auto" => 1, _ => 0 };
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_STROBE)");
            }
            else if (simConnect.IsIni320 || simConnect.IsIni321)
            {
                var code = position switch {  "on" => 0, "auto" => 1, _ => 2 };
                sender.Execute(simConnect, $"{code} (>L:INI_STROBE_LIGHT_SWITCH)");
            }
        }
    }
}
