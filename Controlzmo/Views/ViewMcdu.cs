using Controlzmo.GameControllers;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Views
{
    [Component, RequiredArgsConstructor]
    public partial class ViewMcdu : IButtonCallback<T16000mHotas>
    {
        private readonly CameraState state;
        private readonly CameraView view;

        public int GetButton() => T16000mHotas.BUTTON_FRONT_LEFT_RED;

        public virtual void OnPress(ExtendedSimConnect simConnect) {
            if (state.Current != CameraState.COCKPIT) return;

            var data = new CameraViewData() { viewType = 2, viewIndex = 0 };

            if (simConnect.IsFenix)
                data.viewIndex = view.Current.viewType == 2 && view.Current.viewIndex == 0 ? 1 : 0;
            else if (simConnect.IsFBW)
                // There isn't a right MCDU view, 3 is the closest which is the centre screen.
                data.viewIndex = view.Current.viewType == 2 && view.Current.viewIndex == 4 ? 3 : 4;
            else if (simConnect.IsIni400M)
                data.viewIndex = view.Current.viewType == 2 && view.Current.viewIndex == 10 ? 11 : 10;
            else if (simConnect.IsIniBuilds)
                data.viewIndex = view.Current.viewType == 2 && view.Current.viewIndex == 12 ? 13 : 12;
            else if (simConnect.IsAsoboB38M)
                data.viewIndex = 1;
            else
                 return;

            simConnect.SendDataOnSimObject(data);
        }
    }
}
