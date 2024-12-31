using SimConnectzmo;
using System;

namespace Controlzmo.Systems.Controls
{
    [Component]
    public class AxisSetter
    {
        internal void Set(ExtendedSimConnect sc, IEvent @event, double value)
        {
            /* Note that the brakes are set on a non-linear scale:
                 -16383 = 0% - is this really not -16384?
                  -8191 = 8%
                      0 = 27%
                  +8191 = 53%
                 +16383 = 100% */
            var mapped = 16383 - (int) (32767.0 * value);
            sc.SendEvent(@event, mapped);
        }

        internal void SetAvoidingCentreRepetition(ExtendedSimConnect sc, IEvent @event, double previous, double value, double centre)
        {
            previous = adjust(previous, centre);
            value = adjust(value, centre);
//Console.WriteLine($"{previous:0.0000}/{adjust(previous, centre):0.0000}->{value:0.0000}/{adjust(value, centre):0.0000} with {centre}");
            if (!isCentre(previous) || !isCentre(value))
                Set(sc, @event, value);
        }

        private double adjust(double value, double centre) => value < centre ? 0.5 * value / centre : 1 - (1 - value) / (1 - centre) * 0.5;
        private bool isCentre(double value) => value > 0.49 && value < 0.51;
    }
}
