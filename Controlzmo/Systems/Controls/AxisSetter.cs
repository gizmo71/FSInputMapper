using SimConnectzmo;

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

        internal void SetAvoidingCentreRepetition(ExtendedSimConnect sc, IEvent @event, double old, double @new, double centre, double slack)
        {
            var previous = adjust(old, centre, slack);
            var value = adjust(@new, centre, slack);
//System.Console.WriteLine($"{old:0.0000}/{previous:0.0000}->{@new:0.0000}/{value:0.0000} with {centre}");
            if (previous != value)
                Set(sc, @event, value);
        }

        private double adjust(double value, double centre, double slack) {
            if (isCentre(value, slack)) return 0.5;
            return value < centre ? 0.5 * value / centre : 1 - (1 - value) / (1 - centre) * 0.5;
        }

        private bool isCentre(double value, double slack) => value > (0.5 - slack) && value < (0.5 + slack);
    }
}
