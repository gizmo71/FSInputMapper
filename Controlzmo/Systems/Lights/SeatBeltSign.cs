using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [Component]
    [RequiredArgsConstructor]
    public partial class SeatBeltSign : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public string GetId() => "seatBeltSign";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            sender.Execute(simConnect, simConnect.IsFenix ? $"{desiredValue} (>L:S_OH_SIGNS)" :
                "(A:CABIN SEATBELTS ALERT SWITCH,Bool) " + desiredValue + " != if{ (>K:CABIN_SEATBELTS_ALERT_SWITCH_TOGGLE) }");
        }
    }
}
