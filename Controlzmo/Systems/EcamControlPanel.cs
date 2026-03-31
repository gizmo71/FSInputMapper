using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems
{
    [Component, RequiredArgsConstructor]
    public partial class EcamButtonTakeOffConfig : ISettable<bool>
    {
        private readonly JetBridgeSender jetbridge;

        public string GetId() => "ecamButtonTakeOffConfig";

        public void SetInSim(ExtendedSimConnect simConnect, bool isPressed)
        {
Console.Error.WriteLine($"-->> it be da TOConfig with {isPressed} type");
            var varName = "L:A32NX_BTN_TOCONFIG";
            if (simConnect.IsA32NX)
                varName = $"B:A32NX_PED_ECP_TO_CONF_TEST_PB_{(isPressed ? "Push" : "Release")}";
            else if (simConnect.IsFenix)
                varName = "L:S_ECAM_TO";
            else if (simConnect.IsIniBuilds)
                varName = "L:PUSH_ECAM_TOCONFIG";
            else if (simConnect.IsAtr7x)
                varName = "L:MSATR_ENG_TO_CONFIG";

            jetbridge.Execute(simConnect, $"{(isPressed ? 1 : 0)} (>{varName})");
        }
    }
}
