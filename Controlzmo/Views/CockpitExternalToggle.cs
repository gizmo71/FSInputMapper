﻿using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class CockpitExternalToggleStick : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly CockpitExternalToggleHotas hotas;

        public int GetButton() => UrsaMinorFighterR.BUTTON_SQUARE_HAT_PRESS;

        public void OnPress(ExtendedSimConnect simConnect) => hotas.OnPress(simConnect);
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class CockpitExternalToggleHotas : IButtonCallback<T16000mHotas>
    {
        private readonly ILogger<CockpitExternalToggleHotas> _logger;
        private readonly VirtualJoy vJoy;
        private readonly CameraState state;

        public int GetButton() => T16000mHotas.BUTTON_SIDE_RED;
        public void OnPress(ExtendedSimConnect simConnect)
        {
            if (state.Current == CameraState.CHASE) {
                _logger.LogWarning($"Requesting cockpit for {state.Current}");
                state.Current = CameraState.COCKPIT;
            }  else if (state.Current == CameraState.COCKPIT || state.Current == CameraState.WORLD_MAP) {
                _logger.LogWarning($"Requesting chase for {state.Current}");
                state.Current = CameraState.CHASE;
            } else
                _logger.LogWarning($"Wrong camera state for chase view toggle {state.Current}");
        }
    }
}
