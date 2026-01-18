using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Autothrust
{
    [Component] public class AutoThrottleDisconnectEvent : IEvent { public string SimEvent() => "AUTO_THROTTLE_DISCONNECT"; }

    [Component, RequiredArgsConstructor]
    public partial class AutothrottleDisconnect : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly AutoThrottleDisconnectEvent _event;

        public int GetButton() => UrsaMinorThrottle.BUTTON_AUTOTHRUST_DISCONNECT_RIGHT;

        public virtual void OnPress(ExtendedSimConnect simConnect)
        {
            simConnect.SendEvent(_event, 0u);
        }
    }
}
