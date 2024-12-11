using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Views
{
    [Component, RequiredArgsConstructor]
    public partial class ViewMcdu : IButtonCallback<T16000mHotas>
    {
        public int GetButton() => T16000mHotas.BUTTON_FRONT_LEFT_RED;

        public virtual void OnPress(ExtendedSimConnect simConnect) {
            if (simConnect.IsIniBuilds)
                simConnect.SendDataOnSimObject(new CameraViewData() { viewType = 2, viewIndex = 12 });
        }
    }
}
