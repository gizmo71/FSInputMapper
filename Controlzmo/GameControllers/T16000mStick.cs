using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class T16000mStick : GameController<T16000mStick>
    {
        public T16000mStick(IServiceProvider sp) : base(sp, 16, 4, 1) { }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 45322;

        /* Buttons
        0 trigger
        1 thumb stick top
        2 left side of stick top
        3 right side of stick top
        4/5/6 top left/middle/right left side base
        7/8/9 bottom right/middle/left left side base
        10/11/12 top right/middle/left right side base
        13/14/15 bottom left/middle/right right side base
        /* Axes
        0 roll (0 left, 1 right)
        1 pitch (0 forward, 1 aft)
        2 rudder (twist; 0 left, 1 right)
        3 throttle (0 forward, 1 aft)
        /* Switches
        Nothing registers
        */
        protected override void OnUpdate(ExtendedSimConnect simConnect)
        {
        }
    }
}
