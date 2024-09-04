using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component]
    [RequiredArgsConstructor]
    public partial class PedalDisconnect : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_PINKY;

        public virtual void OnPress(ExtendedSimConnect sc) {
            sender.Execute(sc, "1 (>L:S_FC_CAPT_TILLER_PEDAL_DISCONNECT)");
        }

        public virtual void OnRelease(ExtendedSimConnect sc) {
            sender.Execute(sc, "0 (>L:S_FC_CAPT_TILLER_PEDAL_DISCONNECT)");
        }
    }
}
