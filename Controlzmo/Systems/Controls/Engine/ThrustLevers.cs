using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.Controls.Engine
{
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

        internal void ConvertAndSet(ExtendedSimConnect sc, AbstractThrustLever tl, double @new)
        {
            @new = 1 - @new; // Make -1 full reverse and 1 firewalled.

            int bitmap = 0b1111; // If in doubt, set them all!
            if (data.NumberOfEngines > 2) // Assume four, but sort of works for three too.
                bitmap = tl.LeverNumber == 1 ? 0b0011 : 0b1100;
            else // Assume two, which will work for one too.
                bitmap = tl.LeverNumber == 1 ? 0b0001 : 0b0010;

            double normalised;
            if (sc.IsFBW || sc.IsFenix || sc.IsIniBuilds)
                normalised = AirbusSnap(@new, tl);
            else // The default is to return the non-reverse range as if the reversers were elsewhere.
            {
                normalised = Generic(@new, tl);
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

        private double Generic(double hardware, AbstractThrustLever tl)
        {
            double inputLow = -1;
            double inputHigh = 2;
            double outputLow;
            double outputHigh;

            var position = "?";
            if (hardware < tl.StartRevIdle())
            {
                inputLow = 0.0;
                inputHigh = tl.StartRevIdle();
                outputLow = -1;
                outputHigh = -0.1;
                position = "reverse beyond idle";
            }
            else if (hardware < tl.StartIdle())
            {
                outputLow = outputHigh =  -0.1;
                position = "reverse idle";
            }
            else if (hardware < tl.EndIdle())
            {
                outputLow = outputHigh =  0.0;
                position = "idle";
            }
            else
            {
                inputLow = tl.EndIdle();
                inputHigh = 1;
                outputLow = 0.0;
                outputHigh = 1.0;
                position = "above idle";
            }

            // We could put a 1% margin either side so that it doesn't "jump" out of detents and into ranges, but it's probably not znoticable.
            var positionInRange = (hardware - inputLow) / (inputHigh - inputLow);
            var mapped = positionInRange * (outputHigh - outputLow) + outputLow;
            _logger.LogWarning($"-->>--\t\t{@hardware:0.000} {position} {mapped:+0.000;-0.000; 0.000} for {tl.LeverNumber}");
            return mapped;
        }
//TODO: merge above and below
        private double AirbusSnap(double hardware, AbstractThrustLever tl)
        {
            // Note that the Fenix doesn't do reverse on axis without calibration.
            // If we want to support that, we need a more hybrid approach.
            const double OUTPUT_MAX_REVERSE = -1;
            const double OUTPUT_IDLE_REVERSE = -0.8;
            const double OUTPUT_IDLE = -0.5;
            const double OUTPUT_CLB = 0.001; // Let's try it slightly off 0 to see if that helps
            const double OUTPUT_FLX_MCT = 0.5;
            const double OUTPUT_TOGA = 1;

            double inputLow = -1;
            double inputHigh = 2;
            double outputLow;
            double outputHigh;
            var position = "?";
            if (hardware < tl.EndRevFull())
            {
                outputLow = outputHigh =  OUTPUT_MAX_REVERSE;
                position = "reverse max";
            }
            else if (hardware < tl.StartRevIdle())
            {
                inputLow = tl.EndRevFull();
                inputHigh = tl.StartRevIdle();
                outputLow = OUTPUT_MAX_REVERSE;
                outputHigh = OUTPUT_IDLE_REVERSE;
                position = "manual reverse";
            }
            else if (hardware < tl.StartIdle())
            {
                outputLow = outputHigh =  OUTPUT_IDLE_REVERSE;
                position = "reverse idle";
            }
            else if (hardware < tl.EndIdle())
            {
                outputLow = outputHigh =  OUTPUT_IDLE;
                position = "idle";
            }
            else if (hardware < tl.StartClimb())
            {
                inputLow = tl.EndIdle();
                inputHigh = tl.StartClimb();
                outputLow = OUTPUT_IDLE;
                outputHigh = OUTPUT_CLB;
                position = "manual thrust";
            }
            else if (hardware < tl.EndClimb())
            {
                outputLow = outputHigh =  OUTPUT_CLB;
                position = "CLB";
            }
            else if (hardware < tl.StartFlex())
            {
                inputLow = tl.EndClimb();
                inputHigh = tl.StartFlex();
                outputLow = OUTPUT_CLB;
                outputHigh = OUTPUT_FLX_MCT;
                position = "manual between CLB and FLX";
            }
            else if (hardware < tl.EndFlex())
            {
                outputLow = outputHigh = OUTPUT_FLX_MCT;
                position = "FLX/MCT";
            }
            else
            {
                inputLow = tl.EndFlex();
                inputHigh = 1;
                outputLow = OUTPUT_FLX_MCT;
                outputHigh = OUTPUT_TOGA;
                position = "above FLX";
            }

            // We could put a 1% margin either side so that it doesn't "jump" out of detents and into ranges, but it's probably not znoticable.
            var positionInRange = (hardware - inputLow) / (inputHigh - inputLow);
            var mapped = positionInRange * (outputHigh - outputLow) + outputLow;
            _logger.LogInformation($"-->>--\t\t{@hardware:0.000} {position} {mapped:+0.000;-0.000; 0.000} for {tl.LeverNumber}");
            return mapped;
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
        internal virtual double EndFlex() => 0.865;
    }

    [Component, RequiredArgsConstructor]
    public partial class LeftThrustLever : AbstractThrustLever
    {
        public LeftThrustLever(SetThrustLevers setTLs) : base(setTLs, 1) { }
        public override int GetAxis() => UrsaMinorThrottle.AXIS_THRUST_LEFT;
        internal override double EndRevFull() => 0.06;
        internal override double StartRevIdle() => 0.179;
        internal override double StartIdle() => 0.285;
        internal override double EndIdle() => 0.325;
    }

    [Component, RequiredArgsConstructor]
    public partial class RightThrustLever : AbstractThrustLever
    {
        public RightThrustLever(SetThrustLevers setTLs) : base(setTLs, 2) { }
        public override int GetAxis() => UrsaMinorThrottle.AXIS_THRUST_RIGHT;
        internal override double EndRevFull() => 0.001;
        internal override double StartRevIdle() => 0.130;
        internal override double StartIdle() => 0.275;
        internal override double EndIdle() => 0.31;
    }
}
