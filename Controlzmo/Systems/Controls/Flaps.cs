using Controlzmo.GameControllers;
using Controlzmo.SimConnectzmo;
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
        private readonly InputEvents inputEvents;

        public int GetAxis() => UrsaMinorThrottle.AXIS_FLAPS;

        public void OnChange(ExtendedSimConnect sc, double old, double @new) {
            if (sc.IsFenix || sc.IsAtr7x)
            {
                Interlocked.Exchange(ref discretePosition, @new);
                sender.Execute(sc, Discrete);
            }
            /*else if (sc.IsAtr7x)
                inputEvents.Send(sc, "HANDLING_FLAPS", @new * 32768);*/
            else
                sc.SendEvent(_event, (int)(@new * 32767 - 16383));
        }

        private const double NO_POSITION = -1;
        private double discretePosition = NO_POSITION;
        private String? Discrete(ExtendedSimConnect sc)
        {
            double required = Interlocked.Exchange(ref discretePosition, NO_POSITION);
            if (required == NO_POSITION) return null;
            int raw = (int)((required + 0.1) * 4);
            return sc.IsAtr7x ? $"(>K:FLAPS_{(raw == 0 ? "UP" : raw)})" : $"{raw} (>L:S_FC_FLAPS)";
        }
    }
}
