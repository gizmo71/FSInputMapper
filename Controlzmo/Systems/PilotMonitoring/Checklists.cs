using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [Component, RequiredArgsConstructor]
    public partial class ChecklistPress : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_HAT_PRESS;
        public virtual void OnPress(ExtendedSimConnect sc) => sender.Execute(sc, "1 (>L:A32NX_BTN_CL)");
        public virtual void OnRelease(ExtendedSimConnect sc) => sender.Execute(sc, "0 (>L:A32NX_BTN_CL)");
    }

    [Component, RequiredArgsConstructor]
    public partial class ChecklistUp : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_HAT_UP;
        public virtual void OnPress(ExtendedSimConnect sc) => Set(sc, 1);
        public virtual void OnRelease(ExtendedSimConnect sc) => Set(sc, 0);
        private void Set(ExtendedSimConnect sc, int value) => sender.Execute(sc, $"{value} (>L:{(sc.IsAtr ? "MSATR_EFIS_PROC_UP_1" : "A32NX_BTN_UP")})");
    }

    [Component, RequiredArgsConstructor]
    public partial class ChecklistDown : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_HAT_DOWN;
        public virtual void OnPress(ExtendedSimConnect sc) => Set(sc, 1);
        public virtual void OnRelease(ExtendedSimConnect sc) => Set(sc, 0);
        private void Set(ExtendedSimConnect sc, int value) => sender.Execute(sc, $"{value} (>L:{(sc.IsAtr ? "MSATR_EFIS_PROC_DN_1" : "A32NX_BTN_DOWN")})");
    }

    [Component, RequiredArgsConstructor]
    public partial class ChecklistTick : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_HAT_FORE;
        public virtual void OnPress(ExtendedSimConnect sc) => Set(sc, 1);
        public virtual void OnRelease(ExtendedSimConnect sc) => Set(sc, 0);
        private void Set(ExtendedSimConnect sc, int value) => sender.Execute(sc, $"{value} (>L:{(sc.IsAtr ? "MSATR_EFIS_PROC_VAL_1" : "A32NX_BTN_CHECK_RH")})");
    }

    [Component, RequiredArgsConstructor]
    public partial class ChecklistAbnormal : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_HAT_AFT;
        public virtual void OnPress(ExtendedSimConnect sc) => sender.Execute(sc, "1 (>L:A32NX_BTN_ABNPROC)");
        public virtual void OnRelease(ExtendedSimConnect sc) => sender.Execute(sc, "0 (>L:A32NX_BTN_ABNPROC)");
    }
}
