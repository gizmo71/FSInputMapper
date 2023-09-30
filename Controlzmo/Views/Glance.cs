using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using Windows.Gaming.Input;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Glance : ISwitchCallback<T16000mStick>
    {
        private readonly ILogger<Glance> _log;
        private readonly VirtualJoy vJoy;
        private readonly CameraState cameraState;
        //private readonly CameraView cameraView;
        //private readonly ResetView reset;

        public int GetSwitch() => T16000mStick.SWITCH_TOP_HAT;

        private GameControllerSwitchPosition current = GameControllerSwitchPosition.Center;

        public void OnChange(ExtendedSimConnect simConnect, GameControllerSwitchPosition old, GameControllerSwitchPosition @new)
        {
            _log.LogDebug($"top hat {old}/{current}->{@new}");
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
            if (cameraState.Current.cameraState == CameraState.COCKPIT)
            {
                switch (@new)
                {
                    case GameControllerSwitchPosition.Up:
                        vJoy.getController().QuickClick(103u);
                        break;
                    case GameControllerSwitchPosition.Down:
                        vJoy.getController().QuickClick(107u);
                        break;
                    case GameControllerSwitchPosition.Left:
                        vJoy.getController().QuickClick(104u);
                        break;
                    case GameControllerSwitchPosition.Right:
                        vJoy.getController().QuickClick(106u);
                        break;
                    case GameControllerSwitchPosition.Center:
                        vJoy.getController().QuickClick(105u);
                        break;
                }

            }
            else if (cameraState.Current.cameraState == CameraState.CHASE)
            {
                vJoy.getController().ReleaseButton(110u);
                vJoy.getController().ReleaseButton(111u);
                vJoy.getController().ReleaseButton(112u);
                vJoy.getController().ReleaseButton(113u);
                switch (@new)
                {
                    case GameControllerSwitchPosition.Up:
                        vJoy.getController().PressButton(110u);
                        break;
                    case GameControllerSwitchPosition.Down:
                        vJoy.getController().PressButton(111u);
                        break;
                    case GameControllerSwitchPosition.Left:
                        vJoy.getController().PressButton(113u);
                        break;
                    case GameControllerSwitchPosition.Right:
                        vJoy.getController().PressButton(112u);
                        break;
                }
            }
            else _log.LogDebug($"unhandled camera state {cameraState.Current.cameraState}");
            current = @new;
        }
    }
}
