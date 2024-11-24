using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Views
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ResetCameraData
    {
        [SimVar("CAMERA REQUEST ACTION", null, SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 resetAction;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class ResetView : IData<ResetCameraData>, IButtonCallback<T16000mHotas>
    {
        private readonly VirtualJoy vJoy;
        private readonly CameraState cameraState;

        public int GetButton() => T16000mHotas.BUTTON_MINISTICK;
        public void OnPress(ExtendedSimConnect simConnect) {
            simConnect.SendDataOnSimObject(new ResetCameraData() { resetAction = 1 });
            if (cameraState.Current == CameraState.COCKPIT)
                vJoy.getController().QuickClick(105u);
        }
    }
}
