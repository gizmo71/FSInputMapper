using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

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
        private const Int32 COCKPIT = 2;
        private const Int32 CHASE = 3;

        private readonly ILogger<CockpitExternalToggleHotas> _logger;
        private readonly VirtualJoy vJoy;
        private readonly CameraState state;

        public int GetButton() => T16000mHotas.BUTTON_SIDE_RED;
        public void OnPress(ExtendedSimConnect simConnect)
        {
            if (state.Current.cameraState == COCKPIT || state.Current.cameraState == CHASE) {
                _logger.LogTrace($"Sending 114 for {state.Current.cameraState}");
                vJoy.getController().QuickClick(114u);
            } else
                _logger.LogTrace($"Wrong camera state for chase view toggle {state.Current.cameraState}");
        }
    }
}
