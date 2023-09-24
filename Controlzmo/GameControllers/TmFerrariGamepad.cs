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

        /* Buttons
        0 top right aft
        1 top right left
        2 top right right
        3 top right fore
        4 front top left
        5 front bottom left
        6 front top right
        7 front top right
        8 left underneath
        9 right underneath
        10 left stick
        11 right stick
        /* Axes
        0 left stick roll (0 left->1 right)
        0/1 d-pad roll/pitch (only with bottom light off)
        1 left stick pitch (0 fore->1 aft)
        2 right stick roll (0 left->1 right)
        3 right stick pitch (0 fore->1 aft)
        /* Switches
        0 d-pad roll (only with bottom light on; 0 left->1 right)
        1 d-pad pitch (only with bottom light on; 0 fore->1 aft)
         */
        protected override void OnUpdate(ExtendedSimConnect simConnect) {
        }
    }
}
