using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems
{
    [Component]
    public class EcamButtonTakeOffConfig : ISettable<string?>
    {
        private readonly JetBridgeSender jetbridge;

        public EcamButtonTakeOffConfig(IServiceProvider serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "ecamButtonTakeOffConfig";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            jetbridge.Execute(simConnect, "1 (>L:A32NX_BTN_TOCONFIG)");
        }
    }
}
