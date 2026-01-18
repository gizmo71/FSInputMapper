using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class UrsaMinorThrottle : GameController<UrsaMinorThrottle>
    {
        public UrsaMinorThrottle(IServiceProvider sp) : base(sp, 41, 8, 0) { }

        public override ushort Vendor() => 0x4098;
        public override ushort Product() => 0xB920;

        public static readonly int BUTTON_ENG_MASTER_LEFT_ON = 0;
        public static readonly int BUTTON_ENG_MASTER_LEFT_OFF = 1;
        public static readonly int BUTTON_ENG_MASTER_RIGHT_ON = 2;
        public static readonly int BUTTON_ENG_MASTER_RIGHT_OFF = 3;
        public static readonly int BUTTON_FIRE_LIGHT_LEFT = 4;
        public static readonly int BUTTON_FIRE_LIGHT_RIGHT = 5;
        public static readonly int BUTTON_MODE_CRANK = 6;
        public static readonly int BUTTON_MODE_NORM = 7;
        public static readonly int BUTTON_MODE_START = 8;
        public static readonly int BUTTON_AUTOTHRUST_DISCONNECT_LEFT = 9;
        public static readonly int BUTTON_AUTOTHRUST_DISCONNECT_RIGHT = 10;
        public static readonly int BUTTON_THRUST_CL_TOGA_LEFT = 11;
        public static readonly int BUTTON_THRUST_CL_FLX_MCT_LEFT = 12;
        public static readonly int BUTTON_THRUST_CL_LEFT = 13;
        public static readonly int BUTTON_THRUST_IDLE_LEFT = 14;
        public static readonly int BUTTON_THRUST_REV_IDLE_LEFT = 15;
        public static readonly int BUTTON_THRUST_REV_MAX_LEFT = 16;
        public static readonly int BUTTON_THRUST_CL_TOGA_RIGHT = 17;
        public static readonly int BUTTON_THRUST_CL_FLX_MCT_RIGHT = 18;
        public static readonly int BUTTON_THRUST_CL_RIGHT = 19;
        public static readonly int BUTTON_THRUST_IDLE_RIGHT = 20;
        public static readonly int BUTTON_THRUST_REV_IDLE_RIGHT = 21;
        public static readonly int BUTTON_THRUST_REV_MAX_RIGHT = 22;
        /// <summary>Yep, you can press the mode selector for a hidden button!</summary>
        public static readonly int BUTTON_MODE_PRESS = 23;
        public static readonly int BUTTON_RUDDER_TRIM_RESET = 24;
        public static readonly int BUTTON_RUDDER_TRIM_LEFT = 25;
        public static readonly int BUTTON_RUDDER_TRIM_CENTRE = 26;
        public static readonly int BUTTON_RUDDER_TRIM_RIGHT = 27;
        public static readonly int BUTTON_PARKING_BRAKE_RELEASED = 28;
        public static readonly int BUTTON_PARKING_BRAKE_SET = 29;
        public static readonly int BUTTON_FLAPS_FULL = 30;
        public static readonly int BUTTON_FLAPS_3 = 31;
        public static readonly int BUTTON_FLAPS_2 = 32;
        public static readonly int BUTTON_FLAPS_1 = 33;
        public static readonly int BUTTON_FLAPS_0 = 34;
        /// <summary>True from ~73% to 100%</summary>
        public static readonly int BUTTON_SPEEDBRAKES_FULL = 35;
        /// <summary>True from 50% to ~73%</summary>
        public static readonly int BUTTON_SPEEDBRAKES_HALF = 36;
        /// <summary>True from 0% to &lt;50</summary>
        public static readonly int BUTTON_GROUND_SPOILERS_DISARM = 37;
        public static readonly int BUTTON_GROUND_SPOILERS_ARM = 38;
        /// <summary>Thrust lever anywhere in the reverse range</summary>
        public static readonly int BUTTON_THRUST_REV_RANGE_LEFT = 39;
        /// <summary>Right equivalent of <see cref="BUTTON_THR"/></summary>
        public static readonly int BUTTON_THRUST_REV_RANGE_RIGHT = 40;

        /// <summary>0 is max reverse, 1 is TOGA</summary>
        public static readonly int AXIS_THRUST_LEFT = 3;
        /// <summary>Same range as <see cref="AXIS_THRUST_1"/></summary>
        public static readonly int AXIS_THRUST_RIGHT = 4;
        /// <summary>0 is retracted, 1 is full</summary>
        public static readonly int AXIS_SPEEDBRAKES = 6;
        /// <summary>0 is retracted, ~0.3 is 1, 0.5 is 2, 0.75 is 3, 1 is full</summary>
        public static readonly int AXIS_FLAPS = 7;

        protected override void OnUpdate(ExtendedSimConnect simConnect)
        {
            /*for (int i = 0; i < axesOld.Length; ++i)
                if (axesOld[i] != axesNew[i])
                    Console.Error.WriteLine($"UMT: axes[{i}] {axesOld[i]} -> {axesNew[i]}");
            for (int i = 0; i < this.buttonsOld.Length; ++i)
                if (buttonsOld[i] != buttonsNew[i])
                    Console.Error.WriteLine($"UMT: button {i} now {buttonsNew[i]}");
            for (int i = 0; i < this.switchesOld.Length; ++i)
                if (switchesOld[i] != switchesNew[i])
                    Console.Error.WriteLine($"UMT: switch {i} now {switchesNew[i]}");*/
        }
    }
}
