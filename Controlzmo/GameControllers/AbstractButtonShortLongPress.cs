using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    public abstract class AbstractButtonShortLongPress<T> : IButtonCallback<T> where T : GameController<T>
    {
        private DateTime longAfter = DateTime.MaxValue;
        private delegate void Action(ExtendedSimConnect _);

        public virtual void OnPress(ExtendedSimConnect simConnect) => longAfter = DateTime.UtcNow.AddMilliseconds(350);

        public virtual void OnRelease(ExtendedSimConnect simConnect)
        {
            Action action = (DateTime.UtcNow > longAfter) ? this.OnLongPress : this.OnShortPress;
            action.Invoke(simConnect);
        }

        public abstract int GetButton();
        public abstract void OnLongPress(ExtendedSimConnect simConnect);
        public abstract void OnShortPress(ExtendedSimConnect simConnect);
    }
}
