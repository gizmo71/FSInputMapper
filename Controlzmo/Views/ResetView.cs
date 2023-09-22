using Controlzmo.GameControllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
using System.Threading;

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
        [SimVar("PLANE ALT ABOVE GROUND", "feet", SIMCONNECT_DATATYPE.INT32, 50f)]
        public Int32 terrainHeight;
    }

    [Component]
    public class ResetView : DataListener<ResetViewData>, IButtonCallback
    {
        private readonly VirtualJoy vJoy;

        public ResetView(IServiceProvider sp)
        {
            vJoy = sp.GetRequiredService<VirtualJoy>();
        }

        public int GetButton() => T16000mHotas.BUTTON_MINISTICK;
        public void OnPress(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);

        public override void Process(ExtendedSimConnect simConnect, ResetViewData data)
        {
            simConnect.SendDataOnSimObject(new ResetCameraData() { resetAction = 1 });
            if (data.cameraState == 2) // Cockpit
            {
                var button = data.terrainHeight > 2500 ? 100u : 105u;
                vJoy.getController().ClickButtonAsync(button, 100, new CancellationToken());
            }
        }
    }
}
