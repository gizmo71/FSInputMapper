using SimConnectzmo;
using System;
using System.Threading;

namespace Controlzmo.GameControllers
{
    public abstract class AbstractRepeatingDoublePress
    {
        private Timer? timer;
        private Int16 direction = 0;
        private Boolean isFiring = false;

        public enum Direction:Int16 {  Up = +1, Down = -1 };
        private delegate void Action(ExtendedSimConnect? simConnect);

        private void HandlerTimer(object? simConnect) {
            isFiring = true;
            Action? action = null;
            if (direction == 1)
                 action = UpAction;
            else if (direction == -1)
                 action = DownAction;
            action!.Invoke((ExtendedSimConnect?) simConnect);
        }

        public void Press(ExtendedSimConnect simConnect, Direction newDirection)
        {
            Int16 direction = (Int16) newDirection;
            var isBoth = this.direction == -direction;
            timer?.Dispose();
            isFiring = false;
            if (isBoth)
            {
                this.direction = 0;
                if (!isFiring)
                    BothAction(simConnect);
            }
            else
            {
                this.direction = direction;
                timer = new Timer(HandlerTimer, simConnect, 200, 100);
            }
        }

        public void Release(ExtendedSimConnect simConnect)
        {
            timer?.Dispose();
            if (direction != 0 && !isFiring)
                HandlerTimer(simConnect); // Didn't actually get round to running one, so do it now.
            timer = null;
            direction = 0;
        }

        protected abstract void BothAction(ExtendedSimConnect? simConnect);
        protected abstract void UpAction(ExtendedSimConnect? simConnect);
        protected abstract void DownAction(ExtendedSimConnect? simConnect);
    }

    public interface RepeatingDoublePressButton<T, U> : IButtonCallback<T>
        where T : GameController<T>
        where U : AbstractRepeatingDoublePress
    {
        abstract U Controller { get; }
        AbstractRepeatingDoublePress.Direction GetDirection();
        void IButtonCallback<T>.OnPress(ExtendedSimConnect simConnect) => Controller.Press(simConnect, GetDirection());
        void IButtonCallback<T>.OnRelease(ExtendedSimConnect simConnect) => Controller.Release(simConnect);
    }
}
