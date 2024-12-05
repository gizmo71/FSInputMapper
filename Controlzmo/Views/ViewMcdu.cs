using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;
using System.Threading;
using System.Threading.Tasks;

namespace Controlzmo.Views
{
    [Component, RequiredArgsConstructor]
    public partial class ViewMcdu : IButtonCallback<T16000mHotas>
    {
        private readonly VirtualJoy vJoy;

        public int GetButton() => T16000mHotas.BUTTON_FRONT_LEFT_RED;

        public virtual void OnPress(ExtendedSimConnect simConnect) {
            Task task = vJoy.getController().QuickClick(90u);
            for (int i = 0; i < 3; ++i)
                task  = task.
                    ContinueWith(_ => Thread.Sleep(100)).
                    ContinueWith(_ => vJoy.getController().QuickClick(89u).Wait(300));
        }
    }
}
