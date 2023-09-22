using SimConnectzmo;

namespace Controlzmo.Views
{
    public interface IButtonCallback
    {
        public abstract int GetButton();
        public abstract void OnPress(ExtendedSimConnect _);
    }
}
