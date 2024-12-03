using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems
{
    [Component, RequiredArgsConstructor]
    public partial class EcamButtonTakeOffConfig : ISettable<string?>
    {
        private readonly JetBridgeSender jetbridge;
        private readonly InputEvents inputEvents;

        public string GetId() => "ecamButtonTakeOffConfig";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            String lvar = "A32NX_BTN_TOCONFIG";
            if (simConnect.IsFenix) lvar = "S_ECAM_TO";
            else if (simConnect.IsIniBuilds) lvar = "PUSH_ECAM_TOCONFIG";
            for (int i = 1; i >= 0; i--)
                jetbridge.Execute(simConnect, $"{i} (>L:{lvar})");
        }
    }
}
