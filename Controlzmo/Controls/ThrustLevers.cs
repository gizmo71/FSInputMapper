using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

namespace Controlzmo.Controls
{
    [Component] public class Throttle1Event : IEvent { public string SimEvent() => "THROTTLE1_AXIS_SET_EX1"; }
    [Component] public class Throttle2Event : IEvent { public string SimEvent() => "THROTTLE2_AXIS_SET_EX1"; }
    [Component] public class Throttle3Event : IEvent { public string SimEvent() => "THROTTLE3_AXIS_SET_EX1"; }
    [Component] public class Throttle4Event : IEvent { public string SimEvent() => "THROTTLE4_AXIS_SET_EX1"; }

    [Component, RequiredArgsConstructor]
    public partial class SetThrustLevers
    {
        private readonly ILogger<SetThrustLevers> _logger;
        private readonly Throttle1Event set1;
        private readonly Throttle2Event set2;
        private readonly Throttle3Event set3;
        private readonly Throttle4Event set4;

        internal void Set(ExtendedSimConnect sc, Int32 raw, int bitmap)
        {
            var encoded = BitConverter.ToUInt32(BitConverter.GetBytes(raw), 0);
//_logger.LogError($"Hmm {@new} -> {normalised} -> {raw} -> {encoded:x}");
            if ((bitmap & 1) != 0) sc.SendEvent(set1, encoded);
            if ((bitmap & 2) != 0) sc.SendEvent(set2, encoded);
            if ((bitmap & 4) != 0) sc.SendEvent(set3, encoded);
            if ((bitmap & 8) != 0) sc.SendEvent(set4, encoded);
        }
    }

    [RequiredArgsConstructor]
    public abstract partial class AbstractThrustLever : IAxisCallback<TcaAirbusQuadrant>
    {
        private readonly SetThrustLevers setTLs;
        private readonly int bitmapTwin;
        private readonly int bitmapQuad;

        abstract public int GetAxis();
        public void OnChange(ExtendedSimConnect sc, double old, double @new)
        {
//TODO: was this just because of the trim wheel mapping? private bool isFirstTime = true;
//if (isFirstTime) { isFirstTime = false; return; } //TODO: hmm, why does right hand TL always jump to TOGA at first?!
            //TODO: map somet
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
            //TODO: this based on actual number of engines, rather than specific aircraft.
            setTLs.Set(sc, raw, sc.IsA380X || sc.IsB748 ? bitmapQuad : bitmapTwin);

        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LeftThrustLever : AbstractThrustLever
    {
        public LeftThrustLever(SetThrustLevers setTLs) : base(setTLs, 1, 3) { }
        public override int GetAxis() => TcaAirbusQuadrant.AXIS_LEFT_THRUST;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class RightThrustLever : AbstractThrustLever
    {
        public RightThrustLever(SetThrustLevers setTLs) : base(setTLs, 2, 12) { }
        public override int GetAxis() => TcaAirbusQuadrant.AXIS_RIGHT_THRUST;
    }
}
