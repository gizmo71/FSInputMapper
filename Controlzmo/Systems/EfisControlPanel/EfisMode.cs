using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using SimConnectzmo;
using System;
using System.Numerics;

namespace Controlzmo.Systems.EfisControlPanel
{
    public abstract class EfisMode : ISettable<string>
    {
        protected readonly JetBridgeSender sender;
        protected readonly string id;
        protected readonly string lvarName;

        protected EfisMode(JetBridgeSender sender, string side)
        {
            this.sender = sender;
            id = $"{side}EfisNdMode";
            var sideCode = side.Substring(0, 1).ToUpper();
            lvarName = $"A32NX_EFIS_{sideCode}_ND_MODE";
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            var code = label switch
            {
                "Rose ILS" => 0,
                "Rose VOR" => 1,
                "Rose Nav" => 2,
                "Arc" => 3,
                "Plan" => 4,
                _ => throw new ArgumentOutOfRangeException($"Unrecognised EFIS mode '{label}'")
            };
            sender.Execute(simConnect, $"{code} (>L:{lvarName})");
        }
    }

    [Component]
    public class EfisLeftMode : EfisMode
    {
        public EfisLeftMode(JetBridgeSender sender) : base(sender, "left") { }
    }

    [Component]
    public class EfisRightMode : EfisMode
    {
        public EfisRightMode(JetBridgeSender sender) : base(sender, "right") { }
    }
}
    /*The navaid switches can be read/set using L:A32NX_EFIS_L/R_NAVAID_1/2_MODE (0 off, 1 ADF, 2 VOR).
    The five option buttons can be read/set with L:A32NX_EFIS_L/R_OPTION (0 none, 1 CSTR, 3 WPT, 2 VORD, 4 NDB, 5 ARPT).

    The LS pushbuttons under Baro are BTN_LS_1/2_FILTER_ACTIVE (read/write).
    Where are the FD buttons?    */