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
            vJoy.getController().QuickClick(sticker.IsStuck(500) ? VJoyButton.LOAD_CUSTOM_CAMERA_9: VJoyButton.LOAD_CUSTOM_CAMERA_8);
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
            vJoy.getController().QuickClick(sticker.IsStuck(500) ? VJoyButton.LOAD_CUSTOM_CAMERA_2: VJoyButton.LOAD_CUSTOM_CAMERA_0);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LookEFB : IButtonCallback<T16000mHotas>
    {
        private readonly VirtualJoy vJoy;
        public int GetButton() => T16000mHotas.BUTTON_FRONT_RIGHT_RED;
        public void OnPress(ExtendedSimConnect _) => vJoy.getController().QuickClick(VJoyButton.LOAD_CUSTOM_CAMERA_1);
    }
}
