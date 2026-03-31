using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System.ComponentModel;

namespace Controlzmo.Systems.EfisControlPanel
{
    [Component]
    public class Chrono1Event : IEvent { public string SimEvent() => "A32NX.EFIS_L_CHRONO_PUSHED"; }

    [Component, RequiredArgsConstructor]
    public partial class ChronoButton : ISettable<bool>
    {
        private readonly JetBridgeSender sender;
        private readonly Chrono1Event e;

        public string GetId() => "chrono1press";

        public void PressAndRelease(ExtendedSimConnect simConnect)
        {
            SetInSim(simConnect, true);
            SetInSim(simConnect, false);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool isPressed)
        {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, $"{(isPressed ? 1 : 0)} (>L:S_MIP_CHRONO_CAPT)");
            else if (simConnect.IsIniBuilds && isPressed)
                sender.Execute(simConnect, $"1 (>L:INI_CPT_CHRONO_BUTTON)");
            else if (simConnect.IsAtr7x && isPressed)
                sender.Execute(simConnect, $"1 (>L:MSATR_CLCK_CHRONO_1)");
            else if (simConnect.IsFBW && isPressed)
                simConnect.SendEvent(e);
        }
    }
}
