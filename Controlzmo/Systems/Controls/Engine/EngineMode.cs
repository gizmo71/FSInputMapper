using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls.Engine
{
    [Component] public class EngineModeCrankEvent : IEvent { public string SimEvent() => "ENGINE_MODE_CRANK_SET"; }
    [Component] public class EngineModeNormalEvent : IEvent { public string SimEvent() => "ENGINE_MODE_NORM_SET"; }
    [Component] public class EngineModeStartEvent : IEvent { public string SimEvent() => "ENGINE_MODE_IGN_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class ModeCrank : IButtonCallback<TcaAirbusQuadrant>
    {
        private readonly EngineModeCrankEvent crankEvent;
        private readonly EngineModeNormalEvent normalEvent;

        public int GetButton() => TcaAirbusQuadrant.BUTTON_MODE_CRANK;

        public virtual void OnPress(ExtendedSimConnect simConnect) => simConnect.SendEvent(crankEvent);
        public virtual void OnRelease(ExtendedSimConnect simConnect) => simConnect.SendEvent(normalEvent);
    }

    [Component, RequiredArgsConstructor]
    public partial class ModeStart : IButtonCallback<TcaAirbusQuadrant>
    {
        private readonly EngineModeStartEvent startEvent;
        private readonly EngineModeNormalEvent normalEvent;

        public int GetButton() => TcaAirbusQuadrant.BUTTON_MODE_START;

        public virtual void OnPress(ExtendedSimConnect simConnect) => simConnect.SendEvent(startEvent);
        public virtual void OnRelease(ExtendedSimConnect simConnect) => simConnect.SendEvent(normalEvent);
    }
}
