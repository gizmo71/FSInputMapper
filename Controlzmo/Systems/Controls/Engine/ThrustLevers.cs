using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.Controls.Engine
{
    internal interface TlMapper
    {
        double Map(double input, AbstractThrustLever tl);
    }

    [Component] public class Throttle1Event : IEvent { public string SimEvent() => "THROTTLE1_AXIS_SET_EX1"; }
    [Component] public class Throttle2Event : IEvent { public string SimEvent() => "THROTTLE2_AXIS_SET_EX1"; }
    [Component] public class Throttle3Event : IEvent { public string SimEvent() => "THROTTLE3_AXIS_SET_EX1"; }
    [Component] public class Throttle4Event : IEvent { public string SimEvent() => "THROTTLE4_AXIS_SET_EX1"; }
    [Component] public class Reverse1OnEvent : IEvent { public string SimEvent() => "SET_THROTTLE1_REVERSE_THRUST_ON"; }
    [Component] public class Reverse1OffEvent : IEvent { public string SimEvent() => "SET_THROTTLE1_REVERSE_THRUST_OFF"; }
    [Component] public class Reverse2OnEvent : IEvent { public string SimEvent() => "SET_THROTTLE2_REVERSE_THRUST_ON"; }
    [Component] public class Reverse2OffEvent : IEvent { public string SimEvent() => "SET_THROTTLE2_REVERSE_THRUST_OFF"; }

    [Component, RequiredArgsConstructor]
    public partial class SetThrustLevers
    {
        private readonly ILogger<SetThrustLevers> _logger;
        private readonly Throttle1Event set1;
        private readonly Throttle2Event set2;
        private readonly Throttle3Event set3;
        private readonly Throttle4Event set4;
        private readonly Reverse1OnEvent rev1on;
        private readonly Reverse1OffEvent rev1off;
        private readonly Reverse2OnEvent rev2on;
        private readonly Reverse2OffEvent rev2off;
        private readonly EngineDataListener data;
        private readonly TlAirbus airbusMapper;
        private readonly TlGeneric genericMapper;

        internal void ConvertAndSet(ExtendedSimConnect sc, AbstractThrustLever tl, double @new)
        {
            @new = 1 - @new; // Make -1 full reverse and 1 firewalled.

            int bitmap = 0b1111; // If in doubt, set them all!
            if (data.NumberOfEngines > 2) // Assume four, but sort of works for three too.
                bitmap = tl.LeverNumber == 1 ? 0b0011 : 0b1100;
            else // Assume two, which will work for one too.
                bitmap = tl.LeverNumber == 1 ? 0b0001 : 0b0010;

            double normalised;
            // Note that the ATR needs calibrating in any case.
            if (sc.IsFBW || sc.IsFenix || sc.IsIniBuilds || sc.IsAtr)
                normalised = airbusMapper.Map(@new, tl/*, sc.IsIniBuilds*/);
            else // The default is to return the non-reverse range as if the reversers were elsewhere.
            {
                normalised = genericMapper.Map(@new, tl);
                if (normalised < 0) {
                    normalised = normalised * data.ThrottleLowerLimit; // Looks like we still need a tiny bit of slack, too. :-(
Console.WriteLine($"Rev Value {normalised}  lower limt { data.ThrottleLowerLimit}");
                    sc.SendEvent(tl.LeverNumber == 1 ? rev1on : rev2on);
                }
                else
                    sc.SendEvent(tl.LeverNumber == 1 ? rev1off : rev2off);
                normalised = normalised * 2 - 1;
Console.WriteLine($"Normalised {normalised}");
            }
            _logger.LogTrace($"-->>--\t\t{1 - @new} -> {normalised}");

            var raw = (Int32) (16384 * normalised);
            Set(sc, raw, bitmap);
        }

        private void Set(ExtendedSimConnect sc, Int32 raw, int bitmap)
        {
            if ((bitmap & 1) != 0) sc.SendEvent(set1, raw);
            if ((bitmap & 2) != 0) sc.SendEvent(set2, raw);
            if ((bitmap & 4) != 0) sc.SendEvent(set3, raw);
            if ((bitmap & 8) != 0) sc.SendEvent(set4, raw);
        }
    }

    [RequiredArgsConstructor]
    public abstract partial class AbstractThrustLever : IAxisCallback<UrsaMinorThrottle>
    {
        private readonly SetThrustLevers setTLs;
        private readonly int thrustLeverNumber;

        internal int LeverNumber {  get => thrustLeverNumber; }

        abstract public int GetAxis();
        public void OnChange(ExtendedSimConnect sc, double _, double @new) => setTLs.ConvertAndSet(sc, this, 1 - @new);
// Right hand lever appears to not start to come out of full reverse until left lever is at about 0.05
        internal abstract double EndRevFull();
        internal abstract double StartRevIdle();
        internal abstract double StartIdle();
        internal abstract double EndIdle();
        internal virtual double StartClimb() => 0.67;
        internal virtual double EndClimb() => 0.71;
        internal virtual double StartFlex() => 0.84;
        internal abstract double EndFlex();
    }

    [Component, RequiredArgsConstructor]
    public partial class LeftThrustLever : AbstractThrustLever
    {
        public LeftThrustLever(SetThrustLevers setTLs) : base(setTLs, 1) { }
        public override int GetAxis() => UrsaMinorThrottle.AXIS_THRUST_LEFT;
        internal override double EndRevFull() => 0.060; // This one doesn't report it's whole travel :-(
        internal override double StartRevIdle() => 0.179;
        internal override double StartIdle() => 0.285;
        internal override double EndIdle() => 0.310;
        internal override double EndFlex() => 0.865;
    }

    [Component, RequiredArgsConstructor]
    public partial class RightThrustLever : AbstractThrustLever
    {
        public RightThrustLever(SetThrustLevers setTLs) : base(setTLs, 2) { }
        public override int GetAxis() => UrsaMinorThrottle.AXIS_THRUST_RIGHT;
        internal override double EndRevFull() => 0.001;
        internal override double StartRevIdle() => 0.130;
        internal override double StartIdle() => 0.260;
        internal override double EndIdle() => 0.285;
        internal override double EndFlex() => 0.868;
    }
}
