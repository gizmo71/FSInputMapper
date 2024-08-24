using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component]
    [RequiredArgsConstructor]
    public partial class PedalDisconnect : IButtonCallback<T16000mStick>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => T16000mStick.BUTTON_STICK_TOP_THUMB;

        public virtual void OnPress(ExtendedSimConnect sc) {
            sender.Execute(sc, "1 (>L:S_FC_CAPT_TILLER_PEDAL_DISCONNECT)");
        }

        public virtual void OnRelease(ExtendedSimConnect sc) {
            sender.Execute(sc, "0 (>L:S_FC_CAPT_TILLER_PEDAL_DISCONNECT)");
        }
    }
}
