using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class TmSkyYoke : GameController<TmSkyYoke>
    {
        public TmSkyYoke(IServiceProvider sp) : base(sp, 24, 6, 1) { }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 10245;

        public static readonly int BUTTON_SQUARE_FORE = 0;
        public static readonly int BUTTON_SQUARE_AFT = 1;
        public static readonly int BUTTON_TRIANGLE_FORE = 2;
        public static readonly int BUTTON_TRIANGLE_AFT = 3;
        public static readonly int BUTTON_L1 = 4;
        public static readonly int BUTTON_MINI_TRIM_WHEEL_UP = 5;
        public static readonly int BUTTON_MINI_TRIM_WHEEL_DOWN = 6;
        public static readonly int BUTTON_MINI_TRIM_WHEEL_PRESS = 7;
        public static readonly int BUTTON_MINI_STICK_PRESS = 8;
        public static readonly int BUTTON_R1 = 9;
        public static readonly int BUTTON_X_LEFT = 10;
        public static readonly int BUTTON_X_RIGHT = 11;
        public static readonly int BUTTON_CIRCLE_LEFT = 12;
        public static readonly int BUTTON_CIRCLE_RIGHT = 13;
        public static readonly int BUTTON_ROTARY_LEFTMOST = 14;
        public static readonly int BUTTON_ROTARY_LEFT = 15;
        public static readonly int BUTTON_ROTARY_MIDDLE = 16;
        public static readonly int BUTTON_ROTARY_RIGHT = 17;
        public static readonly int BUTTON_ROTARY_RIGHTMOST = 18;
        public static readonly int BUTTON_BIG_TRIM_WHEEL_UP = 19;
        public static readonly int BUTTON_BIG_TRIM_WHEEL_DOWN = 20;
        public static readonly int BUTTON_BETWEEN_MODE_AND_PS = 21;
        public static readonly int BUTTON_BETWEEN_PS_AND_ROTARY_SELECTOR = 22;
        public static readonly int BUTTON_PS = 23;

        /// <summary>0 left->1 right</summary>
        public static readonly int AXIS_ROLL = 0;
        /// <summary>0 fore->1 aft</summary>
        public static readonly int AXIS_PITCH = 1;
        /// <summary>0 top->1 bottom</summary>
        public static readonly int AXIS_THROTTLE = 2;
        /// <summary>0 left->1 right</summary>
        public static readonly int AXIS_MINI_STICK_X = 3;
        /// <summary>0 fore->1 aft</summary>
        public static readonly int AXIS_MINI_STICK_Y = 4;
        /// <summary>0 left full->0.5 neither->1 right full - they are not independent</summary>
        public static readonly int AXIS_TRIGGERS = 5;

        public static readonly int SWITCH_HAT = 0;

        protected override void OnUpdate(ExtendedSimConnect simConnect) {
            /*for (int i = 0; i < axesOld.Length; ++i)
                if (axesOld[i] != axesNew[i])
                    _log.LogDebug($"SkyYoke: axes[{i}] {axesOld[i]} -> {axesNew[i]}");
            for (int i = 0; i < this.buttonsOld.Length; ++i)
                if (buttonsOld[i] != buttonsNew[i])
                    _log.LogDebug($"SkyYoke: button {i} now {buttonsNew[i]}");
            for (int i = 0; i < this.switchesOld.Length; ++i)
                if (switchesOld[i] != switchesNew[i])
                    _log.LogDebug($"SkyYoke: switch {i} now {switchesNew[i]}");*/
        }
    }
}
