using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;
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
        private UInt32 unstuckView = 105u;

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
            if (cameraState.Current == CameraState.COCKPIT)
            {
                switch (@new)
                {
                    case GameControllerSwitchPosition.Up:
                        sticker.TriggerStart();
                        vJoy.getController().QuickClick(103u);
                        unstuckView = 100u;
                        break;
                    case GameControllerSwitchPosition.Down:
                        sticker.TriggerStart();
                        unstuckView = 108u;
                        vJoy.getController().QuickClick(107u);
                        break;
                    case GameControllerSwitchPosition.Left:
                        vJoy.getController().QuickClick(104u);
                        break;
                    case GameControllerSwitchPosition.Right:
                        vJoy.getController().QuickClick(106u);
                        break;
                    case GameControllerSwitchPosition.Center:
                        vJoy.getController().QuickClick(sticker.IsStuck(350) ? 105u : unstuckView);
                        unstuckView = 105u;
                        break;
                }

            }
            else if (cameraState.Current == CameraState.CHASE)
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
            else _log.LogDebug($"unhandled camera state {cameraState.Current}");
            current = @new;
        }
    }
}
