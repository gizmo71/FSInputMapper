using Lombok.NET;
using Microsoft.Extensions.Logging;

namespace Controlzmo.Systems.Controls.Engine
{
    [Component, RequiredArgsConstructor]
    public partial class TlAirbus : TlMapper
    {
        private readonly ILogger<TlAirbus> _logger;

bool isIniBuilds = true; //TODO: allow better range if not...
        public double Map(double hardware, AbstractThrustLever tl)
        {
            // Note that the Fenix doesn't do reverse on axis without calibration.
            // If we want to support that, we need a more hybrid approach.
            const double OUTPUT_MAX_REVERSE = -1;
            // The iniBuilds A330s and A321LR show reverse selected visually on the levers but the ECAM tells us we need more.
            double OUTPUT_IDLE_REVERSE = isIniBuilds ? -0.89 : -0.8;
            const double OUTPUT_IDLE = -0.5;
            const double OUTPUT_CLB = 0.001; // Let's try it slightly off 0 to see if that helps
            const double OUTPUT_FLX_MCT = 0.5;
            const double OUTPUT_TOGA = 1;

            double inputLow = -1;
            double inputHigh = 2; // Avoid range of 0 when only one output value possible
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
            _logger.LogWarning($"-->>--\t\t{@hardware:0.000} {position} {mapped:+0.000;-0.000; 0.000} for {tl.LeverNumber}");
            return mapped;
        }
    }
}
