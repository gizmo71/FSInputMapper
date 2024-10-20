using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    public abstract class AbstractButtonShortLongPress<T> : IButtonCallback<T> where T : GameController<T>
    {
        private DateTime longAfter = DateTime.MaxValue;

        public virtual void OnPress(ExtendedSimConnect simConnect) => longAfter = DateTime.UtcNow.AddMilliseconds(350);

        public virtual void OnRelease(ExtendedSimConnect simConnect)
        {
            if (DateTime.UtcNow > longAfter)
                OnLongPress(simConnect);
            else
                OnShortPress(simConnect);
        }

        public abstract int GetButton();
        public abstract void OnLongPress(ExtendedSimConnect simConnect);
        public abstract void OnShortPress(ExtendedSimConnect simConnect);
    }
}
