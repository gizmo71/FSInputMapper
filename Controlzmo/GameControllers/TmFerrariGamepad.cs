using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class TmFerrariGamepad : GameController<TmFerrariGamepad>
    {
        public TmFerrariGamepad(IServiceProvider sp) : base(sp, 12, 4, 1) { }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 45845;

        public static readonly int BUTTON_TOP_RIGHT_AFT = 0;
        public static readonly int BUTTON_TOP_RIGHT_LEFT = 1;
        public static readonly int BUTTON_TOP_RIGHT_RIGHT = 2;
        public static readonly int BUTTON_TOP_RIGHT_FORE = 3;
        public static readonly int BUTTON_FRONT_TOP_LEFT = 4;
        public static readonly int BUTTON_FRONT_BOTTOM_LEFT = 5;
        public static readonly int BUTTON_FRONT_TOP_RIGHT = 6;
        public static readonly int BUTTON_FRONT_BOTTOM_RIGHT = 7;
        public static readonly int BUTTON_LEFT_UNDERNEATH = 8;
        public static readonly int BUTTON_RIGHT_UNDERNEATH = 9;
        public static readonly int BUTTON_LEFT_STICK = 10;
        public static readonly int BUTTON_RIGHT_STICK = 11;

        /// <summary>0 left->1 right (with bottom light off)</summary>
        public static readonly int AXIS_LEFT_STICK_ROLL = 0;
        /// <summary>0 fore->1 aft (with bottom light off)</summary>
        public static readonly int AXIS_LEFT_STICK_PITCH = 1;
        /// <summary>0 left->1 right</summary>
        public static readonly int AXIS_RIGHT_STICK_ROLL = 2;
        /// <summary>0 fore->1 aft</summary>
        public static readonly int AXIS_RIGHT_STICK_PITCH = 3;

        /// <summary>0 left->1 right (with bottom light on)</summary>
        public static readonly int SWITCH_DPAD_ROLL = 0;
        /// <summary>0 fore->1 aft (with bottom light on)</summary>
        public static readonly int SWITCH_DPAD_PITCH = 1;

        protected override void OnUpdate(ExtendedSimConnect simConnect) {
        }
    }
}
