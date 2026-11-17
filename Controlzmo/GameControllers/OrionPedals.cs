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

        /// <summary>0 is no brake, 1 is full</summary>
        public static readonly int AXIS_BRAKE_RIGHT = 0;
        /// <summary>0 is no brake, 1 is full</summary>
        public static readonly int AXIS_BRAKE_LEFT = 1;
        /// <summary>0 is left, 1 is right; centre is exactly 0.5</summary>
        public static readonly int AXIS_RUDDER = 2;

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
