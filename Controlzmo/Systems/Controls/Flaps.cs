using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.Linq;
using System.Threading;

namespace Controlzmo.Systems.Controls
{
    [Component] public class FlapsSetEvent : IEvent { public string SimEvent() => "AXIS_FLAPS_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class MoreFlap : IAxisCallback<UrsaMinorThrottle>
    {
        private readonly FlapsSetEvent _event;
        private readonly JetBridgeSender sender;

        public int GetAxis() => UrsaMinorThrottle.AXIS_FLAPS;

        public void OnChange(ExtendedSimConnect sc, double old, double @new) {
            if (sc.IsFenix || sc.IsAtr)
            {
                Interlocked.Exchange(ref discretePosition, @new);
                sender.Execute(sc, Discrete);
            }
            else
            {
                if (sc.IsB78x) @new = MapDreamliner(@new, sc.IsKuroB788);
                sc.SendEvent(_event, (int)(@new * 32767 - 16383));
            }
        }

        private double MapDreamliner(double raw, bool is8)
        {
            // Expessed as a percentage... UP=0 to 30=100
            // 787-9/10: 1=11.11 5=22.22 10=33.33 15=44.44 17=55.56 18=66.67 20=77.78 25=88.89
            // 787-8: 1=16.67 5=33.33 15=50 20=66.67 25=83.33 -- has a bug where the handle positions don't match the systems and we can't get past 18!
            (double, double)[] points = [
                (0.0, 0.0), // Up
                (0.305, is8 ? .3333 : .2222), // 5
                (0.5, is8 ? .5 : .5566), // 17, or 15 for the -8 (which says 10!)
                (0.75, is8 ? .8333 : .8889), // 25 (which says 17 anyway!)
                (1.0, 1.0) // (-8 says 18!)
            ];
            var bottom = points.Where(point => point.Item1 <= raw).Last();
            var top = points.Where(point => point.Item1 >= raw).First();
            var inputRange = top.Item1 - bottom.Item1;
            return Double.Lerp(bottom.Item2, top.Item2, bottom == top ? 0.5 : (raw - bottom.Item1) / inputRange);
        }

        private const double NO_POSITION = -1;
        private double discretePosition = NO_POSITION;
        private String? Discrete(ExtendedSimConnect sc)
        {
            double required = Interlocked.Exchange(ref discretePosition, NO_POSITION);
            if (required == NO_POSITION) return null;
            int raw = (int)((required + 0.1) * 4);
            return sc.IsAtr ? $"(>K:FLAPS_{(raw == 0 ? "UP" : raw)})" : $"{raw} (>L:S_FC_FLAPS)";
        }
    }
}
