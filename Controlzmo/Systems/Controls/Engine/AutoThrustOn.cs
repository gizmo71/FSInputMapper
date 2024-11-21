using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;

namespace Controlzmo.Systems.Controls.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AutothrottleArmedData
    {
        [SimVar("AUTOPILOT THROTTLE ARM", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 autothrottleArmed;
        [SimVar("AUTOTHROTTLE ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 autothrottleActive;
        [SimVar("L:A32NX_AUTOTHRUST_MODE", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxAutothrustMode;
    };

    [Component] public class ToggleAutothrustArmEvent : IEvent { public string SimEvent() => "AUTO_THROTTLE_ARM"; }

    [Component, RequiredArgsConstructor]
    public partial class AutothrottleArmedDataListener : DataListener<AutothrottleArmedData>, IButtonCallback<TcaAirbusQuadrant>
    {
        private readonly ToggleAutothrustArmEvent _event;
        private readonly ILogger<AutothrottleArmedDataListener> _logger;

        public int GetButton() => TcaAirbusQuadrant.BUTTON_LEFT_INTUITIVE_DISCONNECT;

        public virtual void OnPress(ExtendedSimConnect simConnect)
        {
            _logger.LogDebug("User has asked to arm autothrust");
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
        }

        public override void Process(ExtendedSimConnect simConnect, AutothrottleArmedData data)
        {
            _logger.LogDebug($"Is it currently armed? {data.autothrottleArmed}/{data.autothrottleActive} or {data.a32nxAutothrustMode}");
            if (data.autothrottleArmed == 0 && data.autothrottleActive == 0 && data.a32nxAutothrustMode == 0)
                simConnect.SendEvent(_event);
        }
    }
}
