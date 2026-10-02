using Controlzmo.GameControllers;
using Controlzmo.Systems.Controls.Engine;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Autothrust
{
    [Component] public class AutoThrottleDisconnectEvent : IEvent { public string SimEvent() => "AUTO_THROTTLE_DISCONNECT"; }

    [Component, RequiredArgsConstructor]
    public partial class AutothrottleDisconnect : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly AutoThrottleDisconnectEvent _event;
        private readonly AtrPowerMode atrPowerMode;
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorThrottle.BUTTON_AUTOTHRUST_DISCONNECT_RIGHT;

        public virtual void OnPress(ExtendedSimConnect simConnect)
        {
            if (simConnect.IsAtr)
                atrPowerMode.Manipulate(simConnect, 1);
            else if (simConnect.IsB78x)
                sender.Execute(simConnect, "(L:AS01B_AUTO_THROTTLE_ARM_STATE) if{ (>K:AUTO_THROTTLE_ARM) }");
            else
                simConnect.SendEvent(_event, 0u);
        }
    }
}
