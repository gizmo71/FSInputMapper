using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;
using System.Threading;

namespace Controlzmo.Controls
{
    [Component] public class Throttle1Event : IEvent { public string SimEvent() => "THROTTLE1_AXIS_SET_EX1"; }
    [Component] public class Throttle2Event : IEvent { public string SimEvent() => "THROTTLE2_AXIS_SET_EX1"; }

    //TODO: why not? [RequiredArgsConstructor]
    public abstract class AbstractThrustLever : IAxisCallback<TcaAirbusQuadrant>
    {
        private readonly ILogger _logger;
        private readonly IEvent setEvent;
        private readonly int axis;

        protected AbstractThrustLever(ILogger logger, IEvent setEvent, int axis)
        {
            this._logger = logger;
            this.setEvent = setEvent;
            this.axis = axis;
        }

        public int GetAxis() => axis;

        public void OnChange(ExtendedSimConnect sc, double old, double @new)
        {
            double normalised;
            if (sc.IsFBW || sc.IsFenix)
                /*TODO: ought to be something like this in order to put idle at the logcial midpoint:
                normalised = @new <= 0.725 ? 1 - @new / 0.725 : (0.725 - @new) / 0.275;
                In the control settings it look like  full reverse was at -100% (i.e. 1.0), and idle was at +45% (i.e. 1 - 0.725 = 0.275).
                Sadly, it seems I didn't test the idea properly and ended up with the midpoint on the opposite side. */
                normalised = @new <= 0.275 ? 1 - @new / 0.275 : (0.275 - @new) / 0.725;
            else // The default is to return the non-reverse range as if the reversers were elsewhere.
            { 
                normalised = @new < 0.71 ? 1 - 2 * @new / 0.71 : -1;
                if (@new > 0.8) ; //TODO: activate reversers somehow
            }

            var raw = (Int32) (16384 * normalised);
            var encoded = BitConverter.ToUInt32(BitConverter.GetBytes(raw), 0);
_logger.LogError($"Hmm {@new} -> {normalised} -> {raw} -> {encoded:x}");
            sc.SendEvent(setEvent, encoded);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LeftThrustLever : AbstractThrustLever
    {
        public LeftThrustLever(ILogger<RightThrustLever> logger, Throttle1Event setEvent)
            : base(logger, setEvent, TcaAirbusQuadrant.AXIS_LEFT_THRUST)
        {
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class RightThrustLever : AbstractThrustLever
    {
        public RightThrustLever(ILogger<RightThrustLever> logger, Throttle2Event setEvent)
            : base(logger, setEvent, TcaAirbusQuadrant.AXIS_RIGHT_THRUST)
        {
        }
    }
}
