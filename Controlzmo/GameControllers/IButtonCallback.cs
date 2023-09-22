using SimConnectzmo;

namespace Controlzmo.GameControllers
{
    public interface IButtonCallback<C> where C : IGameController
    {
        public abstract int GetButton();
        public virtual void OnPress(ExtendedSimConnect _) { }
        public virtual void OnRelease(ExtendedSimConnect _) { }
    }
}
