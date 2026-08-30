using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.EfisControlPanel
{
    [Component, RequiredArgsConstructor]
    public partial class HudOn : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly HudSet setter;
        public int GetButton() => UrsaMinorFighterR.BUTTON_FAR_TRIGGER_PUSH;
        public virtual void OnPress(ExtendedSimConnect sc) => setter.InUse(sc, true);
    }

    [Component, RequiredArgsConstructor]
    public partial class HudOff : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly HudSet setter;
        public int GetButton() => UrsaMinorFighterR.BUTTON_FAR_TRIGGER_PULL;
        public virtual void OnPress(ExtendedSimConnect sc) => setter.InUse(sc, false);
    }

    [Component, RequiredArgsConstructor]
    public partial class HudSet
    {
        private readonly JetBridgeSender sender;

        internal void InUse(ExtendedSimConnect sc, bool isInUse)
        {
            sender.Execute(sc, $"{(isInUse ? 1 : 0)} (>B:AIRLINER_HUD_1_Set)");
        }
    }
}
