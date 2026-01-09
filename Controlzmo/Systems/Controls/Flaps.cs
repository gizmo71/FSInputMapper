using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component] public class FlapsSetEvent : IEvent { public string SimEvent() => "AXIS_FLAPS_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class MoreFlap : IAxisCallback<UrsaMinorThrottle>
    {
        private readonly FlapsSetEvent _event;
        public int GetAxis() => UrsaMinorThrottle.AXIS_FLAPS;
        public void OnChange(ExtendedSimConnect sc, double old, double @new) => sc.SendEvent(_event, (int)(@new * 32767 - 16383));
    }
}
