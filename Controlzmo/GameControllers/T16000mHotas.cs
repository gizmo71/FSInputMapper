﻿using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class T16000mHotas : GameController<T16000mHotas>
    {
        public T16000mHotas(IServiceProvider sp) : base(sp, 14, 8, 1) { }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 46727;

        /* Axes
        3 right toe brake; 0 max->1 none
        4 left toe brake; 0 max->1 none
        /* Switches
        0 top coolie hat (directions reported as if looking directly at it)
         */
        public static readonly int AXIS_MINISTICK_HORIZONTAL = 0; // 0 left->1 right
        public static readonly int AXIS_MINISTICK_VERTICAL = 1; // 0 up->1 down
        public static readonly int AXIS_THROTTLE = 2; // 1 ide->0 max
        public static readonly int AXIS_RUDDER_PADDLES = 5; // 0 left->1 right
        public static readonly int AXIS_RUDDER_PEDDLES = 6; // 0 left->1 right
        public static readonly int AXIS_WHEEL = 7; // ThrustMaster call it "Antenna"; 0 aft->1 back.

        public static readonly int BUTTON_SIDE_RED = 0;
        public static readonly int BUTTON_FRONT_LEFT_RED = 1;
        public static readonly int BUTTON_FRONT_RIGHT_RED = 2;
        public static readonly int BUTTON_FRONT_ROCKER_UP = 3;
        public static readonly int BUTTON_FRONT_ROCKER_DOWN = 4;
        public static readonly int BUTTON_MINISTICK = 5;
        // 6 middle big "rocker" pad up
        // 7 middle big "rocker" pad fore
        // 8 middle big "rocker" pad down
        // 9 middle big "rocker" pad aft
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
