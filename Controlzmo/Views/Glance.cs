﻿using Controlzmo.GameControllers;
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
            switch (@new)
            {
                case GameControllerSwitchPosition.Left: vJoy.getController().QuickClick(104u); break;
                case GameControllerSwitchPosition.Right: vJoy.getController().QuickClick(106u); break;
// x/y/z, p/b/h
// x: negative left, positive right
// y: positive above, negative below
// z: positive is ahead, negative is behind
// p: positive is down, negative is up
// b: negative anticlockwise, positive clockwise
// h: 0 is forward, -90 left, 90 right
                case GameControllerSwitchPosition.Up: simConnect.CameraSetRelative6DOF(0f, 100f, -15f, 90f, 0f, 0f); break;
                case GameControllerSwitchPosition.Down: simConnect.CameraSetRelative6DOF(0.70f, -2f, -30f, 15f, 0f, 0f); break;
                default:
                    simConnect.SendDataOnSimObject(new ResetViewData() { cameraState = 2 });
                    resetView.OnPress(simConnect);
                    break;
            }
        }
    }
}
