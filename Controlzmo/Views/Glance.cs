using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
using Windows.Gaming.Input;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Glance : ISwitchCallback<T16000mStick>
    {
        private readonly ILogger<Glance> _log;
        private readonly ResetView resetView;
        private readonly VirtualJoy vJoy;

        public int GetSwitch() => T16000mStick.SWITCH_TOP_HAT;

        public void OnChange(ExtendedSimConnect simConnect, GameControllerSwitchPosition old, GameControllerSwitchPosition @new)
        {
            _log.LogCritical($"top hat {old}->{@new}");
            if (@new == GameControllerSwitchPosition.Center)
                resetView.OnPress(simConnect);
            if (@new == GameControllerSwitchPosition.Left)
                vJoy.getController().QuickClick(104u);
            if (@new == GameControllerSwitchPosition.Right)
                vJoy.getController().QuickClick(106u);
        }
    }
}
