using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component] public class MoreFlapEvent : IEvent { public string SimEvent() => "FLAPS_INCR"; }

    [Component, RequiredArgsConstructor]
    public partial class MoreFlap : IButtonCallback<T16000mHotas>
    {
        private readonly MoreFlapEvent _event;
        public int GetButton() => T16000mHotas.BUTTON_BOTTOM_HAT_DOWN;
        public void OnPress(ExtendedSimConnect sc) => sc.SendEvent(_event);
    }

    [Component] public class LessFlapEvent : IEvent { public string SimEvent() => "FLAPS_DECR"; }

    [Component, RequiredArgsConstructor]
    public partial class LessFlap : IButtonCallback<T16000mHotas>
    {
        private readonly LessFlapEvent _event;
        public int GetButton() => T16000mHotas.BUTTON_BOTTOM_HAT_UP;
        public void OnPress(ExtendedSimConnect sc) => sc.SendEvent(_event);
    }
}
