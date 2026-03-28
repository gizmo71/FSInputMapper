using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls.Engine
{
    [Component, RequiredArgsConstructor]
    public partial class EngineStart
    {
        private readonly JetBridgeSender sender;
        internal void Press(ExtendedSimConnect sc, int engine) => Action(sc, engine, 1);
        internal void Release(ExtendedSimConnect sc, int engine) => Action(sc, engine, 0);
        private void Action(ExtendedSimConnect sc, int engine, int value) {
            if (sc.IsAtr7x)
                sender.Execute(sc, $"{value} (>L:MSATR_ENGS_START{engine})");
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class StartButtonLeft : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly EngineStart starter;

        public int GetButton() => UrsaMinorThrottle.BUTTON_FIRE_LIGHT_LEFT;
        public virtual void OnPress(ExtendedSimConnect sc) => starter.Press(sc, 1);
        public virtual void OnRelease(ExtendedSimConnect sc) => starter.Release(sc, 1);
    }

    [Component, RequiredArgsConstructor]
    public partial class StartButtonRight : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly EngineStart starter;

        public int GetButton() => UrsaMinorThrottle.BUTTON_FIRE_LIGHT_RIGHT;
        public virtual void OnPress(ExtendedSimConnect sc) => starter.Press(sc, 2);
        public virtual void OnRelease(ExtendedSimConnect sc) => starter.Release(sc, 2);
    }
}
