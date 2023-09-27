using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.DependencyInjection;
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
    public class ResetCameraAction : IData<ResetCameraData> { }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ResetViewData
    {
        [SimVar("CAMERA STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraState;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class ResetView : DataListener<ResetViewData>, IButtonCallback<T16000mHotas>
    {
        private readonly VirtualJoy vJoy;

        public int GetButton() => T16000mHotas.BUTTON_MINISTICK;
        public void OnPress(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);

        public override void Process(ExtendedSimConnect simConnect, ResetViewData data)
        {
            simConnect.SendDataOnSimObject(new ResetCameraData() { resetAction = 1 });
            if (data.cameraState == 2) // Cockpit
            {
                vJoy.getController().QuickClick(105u);
            }
        }
    }
}
