using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using SimConnectzmo;
using System;
using System.Numerics;

namespace Controlzmo.Systems.EfisControlPanel
{
    public abstract class EfisRange : ISettable<string>
    {
        protected readonly JetBridgeSender sender;
        protected readonly string id;
        protected readonly string lvarName;

        protected EfisRange(JetBridgeSender sender, string side)
        {
            this.sender = sender;
            id = $"{side}EfisNdRange";
            var sideCode = side.Substring(0, 1).ToUpper();
            lvarName = $"A32NX_EFIS_{sideCode}_ND_RANGE";
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            UInt32 range = UInt32.Parse(label!);
            var code = Math.Clamp(BitOperations.Log2(range / 10), 0, 5);
            sender.Execute(simConnect, $"{code} (>L:{lvarName})");
        }
    }

    [Component]
    public class EfisLeftRange : EfisRange
    {
        public EfisLeftRange(JetBridgeSender sender) : base(sender, "left") { }
    }

    [Component]
    public class EfisRightRange : EfisRange
    {
        public EfisRightRange(JetBridgeSender sender) : base(sender, "right") { }
    }
}
