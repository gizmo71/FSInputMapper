using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component, RequiredArgsConstructor]
    public partial class SeatBeltSign : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public string GetId() => "seatBeltSign";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            string? action = null;
            if (simConnect.IsFBW)
                action = "(A:CABIN SEATBELTS ALERT SWITCH,Bool) " + desiredValue + " != if{ (>K:CABIN_SEATBELTS_ALERT_SWITCH_TOGGLE) }";
            else if (simConnect.IsFenix)
                action = $"{desiredValue} (>L:S_OH_SIGNS)";
            else if (simConnect.IsIniBuilds)
            {
                desiredValue = value == true ? 0 : 2;
                if (simConnect.IsIni330) desiredValue = 1 - (desiredValue / 2);
                action = $"{desiredValue} (>L:INI_SEATBELTS_SWITCH)";
            }

            if (action != null)
                sender.Execute(simConnect, action);
        }
    }
}
