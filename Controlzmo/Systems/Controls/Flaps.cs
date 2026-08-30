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
                if (sc.IsHorizonB789) @new = MapDreamliner(@new);
                sc.SendEvent(_event, (int)(@new * 32767 - 16383));
            }
        }

        private double MapDreamliner(double raw)
        {
            // 787-9/10 - what about 8 (has fewer positions)?
            // Expressed as a %, UP=0 1=11.11 5=22.22 10=33.33 15=44.44 17=55.56 18=66.67 20=77.78 25=88.89 30=100
            (double, double)[] points = [
                (0.0, 0.0),
                (0.305, 0.2222),
                (0.5, 0.5566),
                (0.75, 0.8889),
                (1.0, 1.0)
            ];
            var bottom = points.Where(point => point.Item1 <= raw).Last();
            var top = points.Where(point => point.Item1 >= raw).First();
            var inputRange = top.Item1 - bottom.Item1;
            var outputRange = top.Item2 - bottom.Item2;
            var mapped = bottom == top ? bottom.Item2 : (raw - bottom.Item1) / inputRange * outputRange + bottom.Item2;
            return mapped;
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
