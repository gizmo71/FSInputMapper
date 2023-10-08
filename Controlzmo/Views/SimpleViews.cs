using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class LookOverheadPanel : IButtonCallback<T16000mHotas>
    {
        private readonly VirtualJoy vJoy;
        private readonly ViewSticker sticker;
        public int GetButton() => T16000mHotas.BUTTON_FRONT_ROCKER_UP;

        public void OnPress(ExtendedSimConnect _) {
            sticker.TriggerStart();
        }

        public void OnRelease(ExtendedSimConnect _) {
            vJoy.getController().QuickClick(sticker.IsStuck(500) ? 109u: 108u);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LookMainPanelAndPedastal : IButtonCallback<T16000mHotas>
    {
        private readonly VirtualJoy vJoy;
        private readonly ViewSticker sticker;
        public int GetButton() => T16000mHotas.BUTTON_FRONT_ROCKER_DOWN;

        public void OnPress(ExtendedSimConnect _) {
            sticker.TriggerStart();
        }

        public void OnRelease(ExtendedSimConnect _) {
            vJoy.getController().QuickClick(sticker.IsStuck(500) ? 102u: 100u);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LookFlyPad : IButtonCallback<T16000mHotas>
    {
        private readonly VirtualJoy vJoy;
        public int GetButton() => T16000mHotas.BUTTON_FRONT_RIGHT_RED;
        public void OnPress(ExtendedSimConnect _) => vJoy.getController().QuickClick(101u);
    }
}
