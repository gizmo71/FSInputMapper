using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Atc
{
    [Component, RequiredArgsConstructor]
    public partial class AircraftSetup : ISettable<string?>
    {
        private readonly JetBridgeSender sender;

        public string GetId() => "initAircraftState";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            if (simConnect.IsAtr7x)
            {
                sender.Execute(simConnect, "1 (>L:MSATR_MICROPHONE_LEFT_HIDDEN) 1 (>L:MSATR_MICROPHONE_RIGHT_HIDDEN)");
                sender.Execute(simConnect, "1 (>L:XMLVAR_YOKEHIDDEN1) 1 (>L:XMLVAR_YOKEHIDDEN2)");
            }
        }
    }
}
