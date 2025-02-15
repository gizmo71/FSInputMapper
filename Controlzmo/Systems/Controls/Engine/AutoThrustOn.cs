using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Controlzmo.Systems.Controls.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AutothrottleArmedData
    {
        [SimVar("AUTOPILOT THROTTLE ARM", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f)]
        public Int32 autothrottleArmed;
        [SimVar("AUTOTHROTTLE ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f)]
        public Int32 autothrottleActive;
        [SimVar("L:A32NX_AUTOTHRUST_MODE", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f)]
        public Int32 a32nxAutothrustMode;
        [SimVar("L:A32NX_FCU_ATHR_LIGHT_ON", "Number", SIMCONNECT_DATATYPE.INT32, 0.0f)]
        public Int32 a339AutothrustMode;
        [SimVar("L:I_FCU_ATHR", "Bool", SIMCONNECT_DATATYPE.INT32, 0.0f)]
        public Int32 fenixAutothrustArmed;
    };

// Doesn't seem to work in Fenix or Headwind, like it plips on and off...
    [Component] public class ToggleAutothrustArmEvent : IEvent { public string SimEvent() => "AUTO_THROTTLE_ARM"; }

    [Component, RequiredArgsConstructor]
    public partial class AutothrottleArmedDataListener : DataListener<AutothrottleArmedData>, IButtonCallback<TcaAirbusQuadrant>
    {
        private readonly ToggleAutothrustArmEvent _event;
        private readonly ILogger<AutothrottleArmedDataListener> _logger;
        private readonly InputEvents inputEvents;
        private readonly JetBridgeSender sender;

        public int GetButton() => TcaAirbusQuadrant.BUTTON_LEFT_INTUITIVE_DISCONNECT;

        public virtual void OnPress(ExtendedSimConnect simConnect)
        {
            _logger.LogDebug("User has asked to arm autothrust");
            if (simConnect.IsAsoboB38M)
                inputEvents.Send(simConnect, "FCC_AUTOTHROTTLE", 1.0);
            else
                simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
        }

        public virtual void OnRelease(ExtendedSimConnect simConnect)
        {
_logger.LogDebug($"\n\nIs the Fenix button in need of release? {isFenixPressed}");
            if (isFenixPressed)
                ShuffleFenix(simConnect);
            isFenixPressed = false;
        }

        public override void Process(ExtendedSimConnect simConnect, AutothrottleArmedData data)
        {
            if (simConnect.IsFenix)
                data.a32nxAutothrustMode = data.fenixAutothrustArmed;
            else if (simConnect.IsA339)
                data.a32nxAutothrustMode = data.a339AutothrustMode;
_logger.LogDebug($"\n\nIs it currently armed? {data.autothrottleArmed}/{data.autothrottleActive}, A32NX {data.a32nxAutothrustMode}, A339 {data.a339AutothrustMode}, Fenix {data.fenixAutothrustArmed}");
            if (simConnect.IsFenix && false)
            {
                if (data.fenixAutothrustArmed == 0) {
                    ShuffleFenix(simConnect);
                    isFenixPressed = true;
                }
            }
            else if (data.autothrottleArmed == 0 && data.autothrottleActive == 0 && data.a32nxAutothrustMode == 0)
            {
//_logger.LogDebug($"Not Fenix and not armed, so arming");
                simConnect.SendEvent(_event);
            }
else _logger.LogDebug($"Doing nowt");
        }

        private bool isFenixPressed = false;
        private void ShuffleFenix(ExtendedSimConnect simConnect) {
// I think there MUST be an MSFS binding still active...
            //sender.Execute(simConnect, "(L:S_FCU_ATHR) ++ (>L:S_FCU_ATHR)");
        }
    }
}
