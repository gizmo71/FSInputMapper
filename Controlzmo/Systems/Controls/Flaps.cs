using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
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
            if (sc.IsFenix)
            {
                Interlocked.Exchange(ref fenixPosition, @new);
                sender.Execute(sc, Fenix);
            }
            else
                sc.SendEvent(_event, (int)(@new * 32767 - 16383));
        }

        private const double NO_POSITION = -1;
        private double fenixPosition = NO_POSITION;
        private String? Fenix(ExtendedSimConnect _)
        {
            double required = Interlocked.Exchange(ref fenixPosition, NO_POSITION);
            if (required == NO_POSITION) return null;
            int raw = (int)((required + 0.1) * 4);
            return $"{raw} (>L:S_FC_FLAPS)";
        }
    }
}
