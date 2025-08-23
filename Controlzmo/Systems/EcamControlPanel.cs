using Controlzmo.Hubs;
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

        public string GetId() => "ecamButtonTakeOffConfig";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            Func<int, String> varName = (_) => "L:A32NX_BTN_TOCONFIG";
            if (simConnect.IsA32NX)
                varName = (i) => $"B:A32NX_PED_ECP_TO_CONF_TEST_PB_{(i == 1 ? "Push" : "Release")}";
            else if (simConnect.IsFenix)
                varName = (_) => "L:S_ECAM_TO";
            else if (simConnect.IsIniBuilds)
                varName = (_) => "L:PUSH_ECAM_TOCONFIG";

            for (int i = 1; i >= 0; i--)
            {
                jetbridge.Execute(simConnect, $"{i} (>{varName(i)})");
            }
        }
    }
}
