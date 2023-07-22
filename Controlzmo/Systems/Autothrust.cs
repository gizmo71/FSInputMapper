using System;
using System.Runtime.InteropServices;
using Lombok.NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Autothrust
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
// Why does this seem to be so much more complex than it was in the WASM module?!

//TODO: or A32NX.FCU_ATHR_PUSH?
    [Component] public class ToggleAutothrustArmEvent : IEvent { public string SimEvent() => "AUTO_THROTTLE_ARM"; }

    [Component]
    [RequiredArgsConstructor]
    public partial class AutothrottleArmedDataListener : DataListener<AutothrottleArmedData>
    {
        private readonly ToggleAutothrustArmEvent _event;
        private readonly ILogger<AutothrottleArmedDataListener> _logger;

        public override void Process(ExtendedSimConnect simConnect, AutothrottleArmedData data)
        {
            _logger.LogDebug($"Is it currently armed? {data.autothrottleArmed}/{data.autothrottleActive} or {data.a32nxAutothrustMode}");
            if (data.autothrottleArmed == 0 && data.autothrottleActive == 0 && data.a32nxAutothrustMode == 0)
                simConnect.SendEvent(_event);
        }
    }
}
