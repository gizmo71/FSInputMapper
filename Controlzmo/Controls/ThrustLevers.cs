using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Controls
{
    [Component] public class Throttle1Event : IEvent { public string SimEvent() => "THROTTLE1_AXIS_SET_EX1"; }
    [Component] public class Throttle2Event : IEvent { public string SimEvent() => "THROTTLE2_AXIS_SET_EX1"; }
    [Component] public class Throttle3Event : IEvent { public string SimEvent() => "THROTTLE3_AXIS_SET_EX1"; }
    [Component] public class Throttle4Event : IEvent { public string SimEvent() => "THROTTLE4_AXIS_SET_EX1"; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineData
    {
        [SimVar("NUMBER OF ENGINES", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 count;
    };

    [Component, RequiredArgsConstructor]
    public partial class SetThrustLevers : DataListener<EngineData>, IRequestDataOnOpen
    {
        private readonly ILogger<SetThrustLevers> _logger;
        private readonly Throttle1Event set1;
        private readonly Throttle2Event set2;
        private readonly Throttle3Event set3;
        private readonly Throttle4Event set4;
        private int _numberOfEngines;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, EngineData data)
        {
            _numberOfEngines = data.count;
        }

        internal void ConvertAndSet(ExtendedSimConnect sc, AbstractThrustLever tl, double @new)
        {
            int bitmap = 0b1111; // If in doubt, set them all!
            if (_numberOfEngines > 2) // Assume four, but sort of works for three too.
                bitmap = tl.LeverNumber == 1 ? 0b0011 : 0b1100;
            else // Assume two, which will work for one too.
                bitmap = tl.LeverNumber == 1 ? 0b0001 : 0b0010;

            // old/@new - 1 is full reverse, through to 0 at TOGA.
            double normalised;
            if (sc.IsFBW || sc.IsFenix)
                normalised = AirbusSnap(1 - @new, tl);
            else // The default is to return the non-reverse range as if the reversers were elsewhere.
            {
                normalised = @new < 0.71 ? 1 - 2 * @new / 0.71 : -1;
                if (@new > 0.8) ;
            }
            _logger.LogTrace($"-->>--\t\t{1 - @new} -> {normalised}");

            var raw = (Int32) (16384 * normalised);
            Set(sc, raw, bitmap);
        }

        private double AirbusSnap(double hardware, AbstractThrustLever tl)
        {
            const double OUTPUT_MAX_REVERSE = -1;
            const double OUTPUT_IDLE_REVERSE = -0.8;
            const double OUTPUT_IDLE = -0.5;
            const double OUTPUT_CLB = 0;
            const double OUTPUT_FLX_MCT = 0.5;
            const double OUTPUT_TOGA = 1;

            double inputLow = -1;
            double inputHigh = 2;
            double outputLow;
            double outputHigh;
            var position = "?";
            if (hardware < tl.StartRevIdle())
            {
                inputLow = 0;
                inputHigh = tl.StartRevIdle();
                outputLow = OUTPUT_MAX_REVERSE;
                outputHigh = OUTPUT_IDLE_REVERSE;
                position = "reverse beyond idle";
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
            _logger.LogInformation($"-->>--\t\t{@hardware:0.000} {position} {mapped:+0.000} for {tl.LeverNumber}");
            return mapped;
        }

        private void Set(ExtendedSimConnect sc, Int32 raw, int bitmap)
        {
            var encoded = BitConverter.ToUInt32(BitConverter.GetBytes(raw), 0);
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
        private readonly int thrustLeverNumber;

        internal int LeverNumber {  get => thrustLeverNumber; }

        abstract public int GetAxis();
        public void OnChange(ExtendedSimConnect sc, double _, double @new) => setTLs.ConvertAndSet(sc, this, @new);

        internal abstract double StartRevIdle();
        internal abstract double StartIdle();
        internal abstract double EndIdle();
        internal abstract double StartClimb();
        internal abstract double EndClimb();
        internal abstract double StartFlex();
        internal abstract double EndFlex();
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class LeftThrustLever : AbstractThrustLever
    {
        public LeftThrustLever(SetThrustLevers setTLs) : base(setTLs, 1) { }
        public override int GetAxis() => TcaAirbusQuadrant.AXIS_LEFT_THRUST;
        internal override double StartRevIdle() => 0.17;
        internal override double StartIdle() => 0.235;
        internal override double EndIdle() => 0.305;
        internal override double StartClimb() => 0.565;
        internal override double EndClimb() => 0.645;
        internal override double StartFlex() => 0.7;
        internal override double EndFlex() => 0.83;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class RightThrustLever : AbstractThrustLever
    {
        public RightThrustLever(SetThrustLevers setTLs) : base(setTLs, 2) { }
        public override int GetAxis() => TcaAirbusQuadrant.AXIS_RIGHT_THRUST;
        internal override double StartRevIdle() => 0.17;
        internal override double StartIdle() => 0.215;
        internal override double EndIdle() => 0.295;
        internal override double StartClimb() => 0.54;
        internal override double EndClimb() => 0.585;
        internal override double StartFlex() => 0.66;
        internal override double EndFlex() => 0.8;
    }
}
