using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using Windows.Gaming.Input;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Glance : ISwitchCallback<UrsaMinorFighterR>
    {
        private readonly ILogger<Glance> _log;
        private readonly VirtualJoy vJoy;
        private readonly CameraState cameraState;
        private readonly ViewSticker sticker;

        public int GetSwitch() => UrsaMinorFighterR.SWITCH_SQUARE_HAT;

        private GameControllerSwitchPosition current = GameControllerSwitchPosition.Center;
        private VJoyButton unstuckView = VJoyButton.LOAD_CUSTOM_CAMERA_5;

        public void OnChange(ExtendedSimConnect simConnect, GameControllerSwitchPosition old, GameControllerSwitchPosition @new)
        {
            _log.LogDebug($"top hat {old}/{current}->{@new}, camera state {cameraState.Current}");
            switch (@new)
            {
                case GameControllerSwitchPosition.UpLeft:
                case GameControllerSwitchPosition.UpRight:
                case GameControllerSwitchPosition.DownLeft:
                case GameControllerSwitchPosition.DownRight:
                    @new = current;
                    break;
            }
            if (@new == current)
                return;
            if (cameraState.Current == CameraState.COCKPIT || cameraState.Current == CameraState.UNKNOWN)
            {
                switch (@new)
                {
                    case GameControllerSwitchPosition.Up:
                        sticker.TriggerStart();
                        vJoy.getController().QuickClick(VJoyButton.LOAD_CUSTOM_CAMERA_3);
                        unstuckView = VJoyButton.LOAD_CUSTOM_CAMERA_0;
                        break;
                    case GameControllerSwitchPosition.Down:
                        sticker.TriggerStart();
                        unstuckView = VJoyButton.LOAD_CUSTOM_CAMERA_8;
                        vJoy.getController().QuickClick(VJoyButton.LOAD_CUSTOM_CAMERA_7);
                        break;
                    case GameControllerSwitchPosition.Left:
                        vJoy.getController().QuickClick(VJoyButton.LOAD_CUSTOM_CAMERA_4);
                        break;
                    case GameControllerSwitchPosition.Right:
                        vJoy.getController().QuickClick(VJoyButton.LOAD_CUSTOM_CAMERA_6);
                        break;
                    case GameControllerSwitchPosition.Center:
                        vJoy.getController().QuickClick(sticker.IsStuck(350) ? VJoyButton.LOAD_CUSTOM_CAMERA_5 : unstuckView);
                        unstuckView = VJoyButton.LOAD_CUSTOM_CAMERA_5;
                        break;
                }

            }
            else if (cameraState.Current == CameraState.CHASE)
            {
                vJoy.getController().ReleaseButton(VJoyButton.EXTERNAL_QUICKVIEW_TOP);
                vJoy.getController().ReleaseButton(VJoyButton.EXTERNAL_QUICKVIEW_REAR);
                vJoy.getController().ReleaseButton(VJoyButton.EXTERNAL_QUICKVIEW_LEFT);
                vJoy.getController().ReleaseButton(VJoyButton.EXTERNAL_QUICKVIEW_RIGHT);
                switch (@new)
                {
                    case GameControllerSwitchPosition.Up:
                        vJoy.getController().PressButton(VJoyButton.EXTERNAL_QUICKVIEW_TOP);
                        break;
                    case GameControllerSwitchPosition.Down:
                        vJoy.getController().PressButton(VJoyButton.EXTERNAL_QUICKVIEW_REAR);
                        break;
                    case GameControllerSwitchPosition.Left:
                        vJoy.getController().PressButton(VJoyButton.EXTERNAL_QUICKVIEW_RIGHT);
                        break;
                    case GameControllerSwitchPosition.Right:
                        vJoy.getController().PressButton(VJoyButton.EXTERNAL_QUICKVIEW_LEFT);
                        break;
                }
            }
            else _log.LogWarning($"unhandled camera state {cameraState.Current}");
            current = @new;
        }
    }
}
