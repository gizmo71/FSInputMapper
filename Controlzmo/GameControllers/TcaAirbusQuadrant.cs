using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class TcaAirbusQuadrant : GameController<TcaAirbusQuadrant>
    {
        public TcaAirbusQuadrant(IServiceProvider sp) : base(sp, 31, 7, 0) { }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 1031;

        public static readonly int BUTTON_LEFT_INTUITIVE_DISCONNECT = 0;
        public static readonly int BUTTON_RIGHT_INTUITIVE_DISCONNECT = 1;
        public static readonly int BUTTON_LEFT_ENGINE_MASTER = 2;
        public static readonly int BUTTON_RIGHT_ENGINE_MASTER = 3;
        public static readonly int BUTTON_LEFT_FIRE_LIGHT = 4;
        public static readonly int BUTTON_RIGHT_FIRE_LIGHT = 5;
        public static readonly int BUTTON_MODE_CRANK = 6;
        public static readonly int BUTTON_MODE_START = 7;
        public static readonly int AXIS_LEFT_THRUST = 0;
        public static readonly int AXIS_RIGHT_THRUST = 1;

        protected override void OnUpdate(ExtendedSimConnect simConnect)
        {
        }
    }
}
