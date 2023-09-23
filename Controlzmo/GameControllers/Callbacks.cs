using SimConnectzmo;

namespace Controlzmo.GameControllers
{
    public interface IButtonCallback<C> where C : IGameController
    {
        public abstract int GetButton();
        public virtual void OnPress(ExtendedSimConnect _) { }
        public virtual void OnRelease(ExtendedSimConnect _) { }
    }

    public interface ISwitchCallback<C> where C : IGameController
    {
        public abstract int GetSwitch();
        public abstract void OnChange(ExtendedSimConnect _);
    }
}
