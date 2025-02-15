using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;
using Controlzmo.SimConnectzmo;

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

    [Component] public class ToggleAutothrustArmEvent : IEvent { public string SimEvent() => "AUTO_THROTTLE_ARM"; }

    [Component, RequiredArgsConstructor]
    public partial class AutothrottleArmedDataListener : DataListener<AutothrottleArmedData>, IButtonCallback<TcaAirbusQuadrant>
    {
        private readonly ToggleAutothrustArmEvent _event;
        private readonly ILogger<AutothrottleArmedDataListener> _logger;
        private readonly InputEvents inputEvents;

        public int GetButton() => TcaAirbusQuadrant.BUTTON_LEFT_INTUITIVE_DISCONNECT;

        public virtual void OnPress(ExtendedSimConnect simConnect)
        {
            _logger.LogDebug("User has asked to arm autothrust");
            if (simConnect.IsAsoboB38M)
                inputEvents.Send(simConnect, "FCC_AUTOTHROTTLE", 1.0);
            else
                simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
        }

        public override void Process(ExtendedSimConnect simConnect, AutothrottleArmedData data)
        {
            if (simConnect.IsFenix)
                data.autothrottleArmed = data.fenixAutothrustArmed;
            else if (simConnect.IsA339)
                data.autothrottleArmed = data.a339AutothrustMode;
            else if (simConnect.IsA32NX)
                data.autothrottleArmed = data.a32nxAutothrustMode;
            else if (data.autothrottleActive != 0)
                data.autothrottleArmed = 1;
_logger.LogDebug($"\n\nIs it currently armed? {data.autothrottleArmed}/{data.autothrottleActive}, A32NX {data.a32nxAutothrustMode}, A339 {data.a339AutothrustMode}, Fenix {data.fenixAutothrustArmed}");
            if (data.autothrottleArmed == 0)
                simConnect.SendEvent(_event);
        }
    }
}
