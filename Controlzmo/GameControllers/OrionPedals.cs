using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class OrionPedals : GameController<OrionPedals>
    {
        public OrionPedals(IServiceProvider sp) : base(sp, 0, 3, 0) { }

        public override ushort Vendor() => 0x4098;
        public override ushort Product() => PRODUCT_ID;

        public static readonly ushort PRODUCT_ID = 0xBEF0;

        /// <summary>0 is max reverse, 1 is TOGA</summary>
        public static readonly int AXIS_THRUST_LEFT = 1;
        /// <summary>Same range as <see cref="AXIS_THRUST_1"/></summary>
        public static readonly int AXIS_THRUST_RIGHT = 2;
        /// <summary>0 is retracted, 1 is full</summary>
        public static readonly int AXIS_SPEEDBRAKES = 3;

        protected override void OnUpdate(ExtendedSimConnect simConnect)
        {
            for (int i = 0; i < axesOld.Length; ++i)
                if (axesOld[i] != axesNew[i])
                    Console.Error.WriteLine($"UMT: axes[{i}] {axesOld[i]} -> {axesNew[i]}");
            /*for (int i = 0; i < this.buttonsOld.Length; ++i)
                if (buttonsOld[i] != buttonsNew[i])
                    Console.Error.WriteLine($"UMT: button {i} now {buttonsNew[i]}");
            for (int i = 0; i < this.switchesOld.Length; ++i)
                if (switchesOld[i] != switchesNew[i])
                    Console.Error.WriteLine($"UMT: switch {i} now {switchesNew[i]}");*/
        }
    }
}
