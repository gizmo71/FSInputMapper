using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class T16000mHotas : GameController<T16000mHotas>
    {
        public T16000mHotas(IServiceProvider sp) : base(sp, 14, 8, 1) { }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 46727;

        /// <summary>0 left->1 right</summary>
        public static readonly int AXIS_MINISTICK_HORIZONTAL = 0;
        /// <summary>0 up->1 down</summary>
        public static readonly int AXIS_MINISTICK_VERTICAL = 1;
        /// <summary>1 idle->0 max</summary>
        public static readonly int AXIS_THROTTLE = 2;
        /// <summary>0 max->1 none</summary>
        public static readonly int AXIS_TOE_BRAKE_L = 3;
        /// <summary>0 max->1 none</summary>
        public static readonly int AXIS_TOE_BRAKE_R = 4;
        /// <summary>0 left->1 right</summary>
        public static readonly int AXIS_RUDDER_PADDLES = 5;
        /// <summary>0 left->1 right</summary>
        public static readonly int AXIS_RUDDER_PEDALS = 6;
        /// <summary>ThrustMaster call it "Antenna"; 0 aft->1 back</summary>
        public static readonly int AXIS_WHEEL = 7;

        /// <summary>Directions reported as if looking directly at it</summary>
        public static readonly int SWITCH_TOP_COOLIE_HAT = 7;

        public static readonly int BUTTON_SIDE_RED = 0;
        public static readonly int BUTTON_FRONT_LEFT_RED = 1;
        public static readonly int BUTTON_FRONT_RIGHT_RED = 2;
        public static readonly int BUTTON_FRONT_ROCKER_UP = 3;
        public static readonly int BUTTON_FRONT_ROCKER_DOWN = 4;
        public static readonly int BUTTON_MINISTICK = 5;
        public static readonly int BUTTON_MID_BIG_ROCKER_PAD_UP = 6;
        public static readonly int BUTTON_MID_BIG_ROCKER_PAD_FORE = 7;
        public static readonly int BUTTON_MID_BIG_ROCKER_PAD_AFT = 8;
        public static readonly int BUTTON_MID_BIG_ROCKER_PAD_DOWN = 9;
        // Bottom hat or "castle".
        public static readonly int BUTTON_BOTTOM_HAT_UP = 10;
        public static readonly int BUTTON_BOTTOM_HAT_FORE = 11;
        public static readonly int BUTTON_BOTTOM_HAT_DOWN = 12;
        public static readonly int BUTTON_BOTTOM_HAT_AFT = 13;
        protected override void OnUpdate(ExtendedSimConnect simConnect)
        {
        }

    }
}
