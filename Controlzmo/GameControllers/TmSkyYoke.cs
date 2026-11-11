using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class TmSkyYoke : GameController<TmSkyYoke>
    {
        public TmSkyYoke(IServiceProvider sp) : base(sp, 24, 8, 1) { }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 10245;

        public static readonly int BUTTON_BLA = 0;

        /// <summary>0 left->1 right (with bottom light off)</summary>
        public static readonly int AXIS_BLA = 0;

        /// <summary>0 left->1 right (with bottom light on)</summary>
        public static readonly int SWITCH_BLA = 0;

        protected override void OnUpdate(ExtendedSimConnect simConnect) {
        }
    }
}
