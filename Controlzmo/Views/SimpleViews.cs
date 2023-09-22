﻿using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class LookOverheadPanel : IButtonCallback
    {
        private readonly VirtualJoy vJoy;
        public int GetButton() => T16000mHotas.BUTTON_FRONT_ROCKER_UP;
        public void OnPress(ExtendedSimConnect _) => vJoy.getController().QuickClick(109u);
        public void OnRelease(ExtendedSimConnect _) => vJoy.getController().QuickClick(108u);
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LookMainPanelAndPedastal : IButtonCallback
    {
        private readonly VirtualJoy vJoy;
        public int GetButton() => T16000mHotas.BUTTON_FRONT_ROCKER_DOWN;
        public void OnPress(ExtendedSimConnect _) => vJoy.getController().QuickClick(102u);
        public void OnRelease(ExtendedSimConnect _) => vJoy.getController().QuickClick(100u);
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LookFlyPad : IButtonCallback
    {
        private readonly VirtualJoy vJoy;
        public int GetButton() => T16000mHotas.BUTTON_FRONT_RIGHT_RED;
        public void OnPress(ExtendedSimConnect _) => vJoy.getController().QuickClick(101u);
    }
}
