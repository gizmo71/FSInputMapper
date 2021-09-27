using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.EfisControlPanel
{
    public abstract class EfisNavAid : ISettable<string>
    {
        protected readonly JetBridgeSender sender;
        protected readonly string id;
        protected readonly string lvarName;

        protected EfisNavAid(JetBridgeSender sender, string side, int number)
        {
            this.sender = sender;
            id = $"{side}EfisNavAid{number}";
            var sideCode = side.Substring(0, 1).ToUpper();
            lvarName = $"A32NX_EFIS_{sideCode}_NAVAID_{number}_MODE";
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            var code = label switch
            {
                "Off" => 0,
                "ADF" => 1,
                "VOR" => 2,
                _ => throw new ArgumentOutOfRangeException($"Unrecognised EFIS navaid setting '{label}'")
            };
            sender.Execute(simConnect, $"{code} (>L:{lvarName})");
        }
    }

    [Component]
    public class EfisLeftNavAid1 : EfisNavAid
    {
        public EfisLeftNavAid1(JetBridgeSender sender) : base(sender, "left", 1) { }
    }

    [Component]
    public class EfisLeftNavAid2 : EfisNavAid
    {
        public EfisLeftNavAid2(JetBridgeSender sender) : base(sender, "left", 2) { }
    }

    [Component]
    public class EfisRightNavAid1 : EfisNavAid
    {
        public EfisRightNavAid1(JetBridgeSender sender) : base(sender, "right", 1) { }
    }

    [Component]
    public class EfisRightNavAid2 : EfisNavAid
    {
        public EfisRightNavAid2(JetBridgeSender sender) : base(sender, "right", 2) { }
    }
}
