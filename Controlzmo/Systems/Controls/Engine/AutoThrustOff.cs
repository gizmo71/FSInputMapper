using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Autothrust
{
    [Component] public class AutoThrottleDisconnectEvent : IEvent { public string SimEvent() => "AUTO_THROTTLE_DISCONNECT"; }

    [Component, RequiredArgsConstructor]
    public partial class AutothrottleDisconnect : IButtonCallback<TcaAirbusQuadrant>
    {
        private readonly AutoThrottleDisconnectEvent _event;

        public int GetButton() => TcaAirbusQuadrant.BUTTON_RIGHT_INTUITIVE_DISCONNECT;

        public virtual void OnPress(ExtendedSimConnect simConnect)
        {
            simConnect.SendEvent(_event, 0u);
        }
    }
}
