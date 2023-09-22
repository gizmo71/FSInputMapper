using SimConnectzmo;

namespace Controlzmo.GameControllers
{
    public interface IButtonCallback
    {
        public abstract int GetButton();
        public virtual void OnPress(ExtendedSimConnect _) { }
        public virtual void OnRelease(ExtendedSimConnect _) { }
    }
}
