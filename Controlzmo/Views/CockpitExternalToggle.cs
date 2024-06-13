using Controlzmo.GameControllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

namespace Controlzmo.Views
{
    [Component]
    public class CockpitExternalToggle : IButtonCallback<T16000mHotas>
    {
        private const Int32 COCKPIT = 2;
        private const Int32 CHASE = 3;

        protected readonly ILogger _logger;
        private readonly VirtualJoy vJoy;
        private readonly CameraState state;

        public CockpitExternalToggle(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<CockpitExternalToggle>>();
            state = sp.GetRequiredService<CameraState>();
            vJoy = sp.GetRequiredService<VirtualJoy>();
        }

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
