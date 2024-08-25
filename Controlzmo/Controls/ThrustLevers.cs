using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

namespace Controlzmo.Controls
{
    [Component] public class Throttle3Event : IEvent { public string SimEvent() => "THROTTLE3_AXIS_SET_EX1"; }

    [Component]
    [RequiredArgsConstructor]
    public partial class LeftThrustLever : IAxisCallback<TcaAirbusQuadrant>
    {
        private readonly Throttle3Event setEvent;
        private readonly ILogger<Throttle3Event> _logger;

        public int GetAxis() => TcaAirbusQuadrant.AXIS_LEFT_THRUST;

        public void OnChange(ExtendedSimConnect sc, double old, double @new)
        {
            if (!sc.IsB748) return;
            var input = (Int32) (16384.0 - 32768 * @new);
            var raw = BitConverter.ToUInt32(BitConverter.GetBytes(input), 0);
_logger.LogError($"Hmm {@new} -> {input} -> {raw:x}");
            sc.SendEvent(setEvent, raw);
        }
    }
}
