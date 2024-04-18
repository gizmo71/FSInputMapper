using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    [RequiredArgsConstructor]
    public partial class WingIceLight : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public string GetId() => "wingIceLight";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            sender.Execute(simConnect, simConnect.IsFenix ?
                $"{desiredValue} (>L:S_OH_EXT_LT_WING)" :
                $"(A:LIGHT WING, Bool) {desiredValue} != if{{ 0 (>K:TOGGLE_WING_LIGHTS) }}");
        }
    }
}
