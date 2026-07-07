using Lombok.NET;
using Microsoft.Extensions.Logging;

namespace Controlzmo.Systems.Controls.Engine
{
    [Component, RequiredArgsConstructor]
    public partial class TlGeneric : TlMapper
    {
        private readonly ILogger<TlGeneric> _logger;

        public double Map(double hardware, AbstractThrustLever tl)
        {
            double inputLow = -1;
            double inputHigh = 2; // Avoid range of 0 when only one output value possible
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

            // We could put a 1% margin either side so that it doesn't "jump" out of detents and into ranges, but it's probably not noticable.
            var positionInRange = (hardware - inputLow) / (inputHigh - inputLow);
            var mapped = positionInRange * (outputHigh - outputLow) + outputLow;
            _logger.LogWarning($"-->>--\t\t{@hardware:0.000} {position} {mapped:+0.000;-0.000; 0.000} for {tl.LeverNumber}");
            return mapped;
        }
    }
}
