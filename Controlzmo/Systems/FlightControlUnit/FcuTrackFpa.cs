using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component, RequiredArgsConstructor]
    public partial class FcuTrackFpaToggled : IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_TRK_FPA_TOGGLE_PUSH";

        public void Toggle(ExtendedSimConnect simConnect, bool favourLateralMode)
        {
            if (simConnect.IsFenix)
                for (int i = 0; i < 2; ++i)
                    sender.Execute(simConnect, "(L:S_FCU_HDGVS_TRKFPA) ++ (>L:S_FCU_HDGVS_TRKFPA)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_FCU_HDG_VS_COMMAND)");
            else if (simConnect.IsB78x)
                sender.Execute(simConnect, favourLateralMode ? "(>B:AIRLINER_HDG_TRK_Toggle)" : "(>B:AIRLINER_VS_FPA_Toggle)");
            else // FBW etc
                simConnect.SendEvent(this);
        }

    }
}
