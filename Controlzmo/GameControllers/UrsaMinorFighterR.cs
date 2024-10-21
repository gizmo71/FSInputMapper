using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class UrsaMinorFighterR : GameController<UrsaMinorFighterR>
    {
        public UrsaMinorFighterR(IServiceProvider sp) : base(sp, 52, 10, 10) { }

        public override ushort Vendor() => 16536;
        public override ushort Product() => 48170;

        public static readonly int BUTTON_LEFT_BASE_FAR_LEFT_UP = 0;
        public static readonly int BUTTON_LEFT_BASE_FAR_RIGHT_UP = 1;
        public static readonly int BUTTON_LEFT_BASE_FAR_LEFT_DOWN = 2;
        public static readonly int BUTTON_LEFT_BASE_FAR_RIGHT_DOWN = 3;
        public static readonly int BUTTON_LEFT_BASE_ROUND = 4;
        public static readonly int BUTTON_LEFT_BASE_NEAR_UP_ = 5;
        public static readonly int BUTTON_LEFT_BASE_NEAR_DOWN = 6;
        public static readonly int BUTTON_RIGHT_BASE_FAR_RIGHT_UP = 7;
        public static readonly int BUTTON_RIGHT_BASE_FAR_LEFT_UP = 8;
        public static readonly int BUTTON_RIGHT_BASE_FAR_RIGHT_DOWN = 9;
        public static readonly int BUTTON_RIGHT_BASE_FAR_LEFT_DOWN = 10;
        public static readonly int BUTTON_RIGHT_BASE_ROUND = 11;
        public static readonly int BUTTON_RIGHT_BASE_NEAR_UP = 12;
        public static readonly int BUTTON_RIGHT_BASE_NEAR_DOWN = 13;
        public static readonly int BUTTON_RED = 19;
        public static readonly int BUTTON_SQUARE_HAT_PRESS = 20;
        public static readonly int BUTTON_SMALLER_ROUND = 21;
        public static readonly int BUTTON_RIGHT_HAT_PRESS = 22;
        public static readonly int BUTTON_RIGHT_HAT_FORE = 23;
        public static readonly int BUTTON_RIGHT_HAT_DOWN = 24;
        public static readonly int BUTTON_RIGHT_HAT_AFT = 25;
        public static readonly int BUTTON_RIGHT_HAT_UP = 26;
        public static readonly int BUTTON_LARGER_ROUND = 27;
        // "Trim" is the thing that looks a bit like a wheel inside guards in the bottom-middle of the stick's "head".
        public static readonly int BUTTON_TRIM_PRESS = 28;
        public static readonly int BUTTON_TRIM_UP = 29;
        public static readonly int BUTTON_TRIM_RIGHT = 30;
        public static readonly int BUTTON_TRIM_DOWN = 31;
        public static readonly int BUTTON_TRIM_LEFT = 32;
        public static readonly int BUTTON_MINI_STICK_PRESS = 33;
        public static readonly int BUTTON_FAR_TRIGGER_PULL = 34;
        public static readonly int BUTTON_FAR_TRIGGER_PUSH = 35;
        public static readonly int BUTTON_NEAR_TRIGGER_HALF = 36; // Remains on when trigger full
        public static readonly int BUTTON_NEAR_TRIGGER_FULL = 37;
        public static readonly int BUTTON_PINKY = 38;
        public static readonly int BUTTON_MID_STICK_TRIM_PRESS = 39;
        public static readonly int BUTTON_MID_STICK_TRIM_FORE = 40;
        public static readonly int BUTTON_MID_STICK_TRIM_RIGHT = 41;
        public static readonly int BUTTON_MID_STICK_TRIM_AFT = 42;
        public static readonly int BUTTON_MID_STICK_TRIM_LEFT = 43;
/* According to the pictures in the manual, we should also see (needs their software?):
44-47 up/right/down/left on mini-stick axis
48-50 left/centre/right on twist axis
*/
        public static readonly int AXIS_ROLL = 0;
        public static readonly int AXIS_PITCH = 1;
        public static readonly int AXIS_TWIST = 2;
        public static readonly int AXIS_MINI_STICK_X = 3;
        public static readonly int AXIS_MINI_STICK_Y = 4;
        public static readonly int AXIS_THROTTLE = 5;

        public static readonly int SWITCH_SQUARE_HAT = 0;

        protected override void OnUpdate(ExtendedSimConnect simConnect)
        {
            /*for (int i = 0; i < axesOld.Length; ++i)
                if (axesOld[i] != axesNew[i])
                    _log.LogDebug($"UMFR: axes[{i}] {axesOld[i]} -> {axesNew[i]}");
            for (int i = 0; i < this.buttonsOld.Length; ++i)
                if (buttonsOld[i] != buttonsNew[i])
                    _log.LogDebug($"UMFR: button {i} now {buttonsNew[i]}");
            for (int i = 0; i < this.switchesOld.Length; ++i)
                if (switchesOld[i] != switchesNew[i])
                    _log.LogDebug($"UMFR: switch {i} now {switchesNew[i]}");*/
        }

    }
}
