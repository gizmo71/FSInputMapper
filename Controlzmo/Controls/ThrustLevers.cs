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
            // old/@new - 1 is full reverse, through to 0 at TOGA.
            double normalised;
            if (sc.IsFBW || sc.IsFenix)
                normalised = AirbusSnap(1 - @new);
            else // The default is to return the non-reverse range as if the reversers were elsewhere.
            {
                normalised = @new < 0.71 ? 1 - 2 * @new / 0.71 : -1;
                if (@new > 0.8) ; //TODO: activate reversers somehow
            }
System.Console.WriteLine($"-->>--\t\t{1 - @new} -> {normalised}");

            var raw = (Int32) (16384 * normalised);
            //TODO: this based on actual number of engines, rather than specific aircraft.
            setTLs.Set(sc, raw, sc.IsA380X || sc.IsB748 ? bitmapQuad : bitmapTwin);
        }

        private double AirbusSnap(double hardware)
        {
// Fenix appears to use defaults starting at 0, not -1.
// https://github.com/flybywiresim/aircraft/blob/fa3414778ec26b1a2c7cbc6cdc1d38a2800aff3a/fbw-a32nx/docs/Configuration/ThrottleConfiguration.ini
            const double OUTPUT_MAX_REVERSE = -1;
            const double OUTPUT_IDLE_REVERSE = -0.2;
            const double OUTPUT_IDLE = 0;
            const double OUTPUT_CLB = 0.7;
            const double OUTPUT_FLX_MCT = 0.85;
            const double OUTPUT_TOGA = 1;

            double inputLow = 0.275;
            double inputHigh = 1;
            double outputLow = 0;
            double outputHigh = 1;
            if (hardware < 0.185) // 0.20 (L) 0.19 (R) idle reverse
            {
                inputLow = 0;
                inputHigh = 0.185;
                outputLow = OUTPUT_MAX_REVERSE;
                outputHigh = OUTPUT_IDLE_REVERSE;
            }
            else if (hardware < 0.20) // 0.255 (L) 0.23 (R) idle
            {
                return OUTPUT_IDLE_REVERSE;
            }
            else if (hardware < 0.26)
            {
                return OUTPUT_IDLE;
            }
            else if (hardware < 0.5) // 0.607 (L) 0.55 (R) CLB
            {
                inputLow = 0.26;
                inputHigh = 0.5;
                outputLow = OUTPUT_IDLE;
                outputHigh = OUTPUT_CLB;
            }
            else if (hardware < 0.65)
            {
                return OUTPUT_CLB;
            }
            else if (hardware < 0.66) // 0.782 (L) 0.704 (R) FLX/MCT
            {
                inputLow = 0.495;
                inputHigh = 0.66;
                outputLow = OUTPUT_CLB;
                outputHigh = OUTPUT_FLX_MCT;
            }
            else if (hardware < 0.83)
            {
                return OUTPUT_FLX_MCT;
            }
            else
            {
                inputLow = 0.83;
                inputHigh = 1;
                outputLow = OUTPUT_FLX_MCT;
                outputHigh = OUTPUT_TOGA;
            }
// Snap zone probably about 0.05 around each...
            var positionInRange = (hardware - inputLow) / (inputHigh - inputLow);
            return positionInRange * (outputHigh - outputLow) + outputLow;
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
