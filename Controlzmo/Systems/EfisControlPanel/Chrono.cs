using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System.ComponentModel;

namespace Controlzmo.Systems.EfisControlPanel
{
    [Component]
    public class Chrono1Event : IEvent { public string SimEvent() => "A32NX.EFIS_L_CHRONO_PUSHED"; }

    [Component]
    [RequiredArgsConstructor]
    public partial class ChronoButton : ISettable<object>
    {
        private readonly JetBridgeSender sender;
        private readonly Chrono1Event e;

        public string GetId() => "chrono1press";

        public void SetInSim(ExtendedSimConnect simConnect, object? value)
        {
            if (simConnect.IsFenix)
            {
                for (var i = 1; i >= 0; --i)
                    sender.Execute(simConnect, $"{i} (>L:S_MIP_CHRONO_CAPT)");
            }
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, $"1 (>L:INI_CPT_CHRONO_BUTTON)");
            else if (simConnect.IsFBW)
                simConnect.SendEvent(e);
        }
    }
}
