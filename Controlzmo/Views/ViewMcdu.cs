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
            CameraViewData data;
            if (simConnect.IsIniBuilds)
                data = new CameraViewData() { viewType = 2, viewIndex = 12 };
            else if (simConnect.IsAsoboB38M)
                data = new CameraViewData() { viewType = 2, viewIndex = 1 };
            else if (simConnect.IsA380X)
                data = new CameraViewData() { viewType = 2, viewIndex = 4 };
            else
                 return;
            simConnect.SendDataOnSimObject(data);
        }
    }
}
