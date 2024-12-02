using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FcuTrackFpaToggled : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_TRK_FPA_TOGGLE_PUSH";
        public string GetId() => "trkFpaToggled";

        public void SetInSim(ExtendedSimConnect simConnect, bool _)
        {
            if (simConnect.IsFenix)
                for (int i = 0; i < 2; ++i)
                    sender.Execute(simConnect, "(L:S_FCU_HDGVS_TRKFPA) ++ (>L:S_FCU_HDGVS_TRKFPA)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_FCU_HDG_VS_COMMAND)");
            else
                simConnect.SendEvent(this);
        }

    }
}
