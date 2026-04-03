using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Atc
{
    [Component] public class Com1VolumeSetEvent : IEvent { public string SimEvent() => "COM1_VOLUME_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class AircraftSetup : ISettable<string?>
    {
        private readonly JetBridgeSender sender;
        private readonly Com1VolumeSetEvent volume;

        public string GetId() => "initAircraftState";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            simConnect.SendEvent(volume, 100);
            if (simConnect.IsAtr7x)
            {
                sender.Execute(simConnect, "1 (>L:MSATR_MICROPHONE_LEFT_HIDDEN) 1 (>L:MSATR_MICROPHONE_RIGHT_HIDDEN)");
                sender.Execute(simConnect, "1 (>L:XMLVAR_YOKEHIDDEN1) 1 (>L:XMLVAR_YOKEHIDDEN2)");
            }
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, "0 (>L:S_EFB_VISIBLE_FO) 0 (>L:S_EFB_CHARGING_CABLE_FO) 0 (>L:S_WINDOW_BLINDS_FO) 1.0 (>L:A_MIP_LIGHTING_FLOOD_MAIN)");
        }
    }
}
