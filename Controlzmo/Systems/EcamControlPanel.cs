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
            String lvar = simConnect.IsFenix ? "S_ECAM_TO" : "A32NX_BTN_TOCONFIG";
            for (int i = 1; i >= 0; i--)
                jetbridge.Execute(simConnect, $"{i} (>L:{lvar})");
        }
    }
}
