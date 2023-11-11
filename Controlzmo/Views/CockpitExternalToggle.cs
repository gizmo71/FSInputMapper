using Controlzmo.GameControllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Views
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CockpitExternalToggleData
    {
        [SimVar("CAMERA STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraState;
    }

    [Component]
    public class CockpitExternalToggle : DataListener<CockpitExternalToggleData>, IButtonCallback<T16000mHotas>
    {
        private const Int32 COCKPIT = 2;
        private const Int32 CHASE = 3;

        protected readonly ILogger _logger;
        private readonly VirtualJoy vJoy;

        public CockpitExternalToggle(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<CockpitExternalToggle>>();
            vJoy = sp.GetRequiredService<VirtualJoy>();
        }

        public int GetButton() => T16000mHotas.BUTTON_SIDE_RED;
        public void OnPress(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);

        public override void Process(ExtendedSimConnect simConnect, CockpitExternalToggleData data)
        {
            if (data.cameraState == COCKPIT || data.cameraState == CHASE) {
_logger.LogWarning($"Sedning 114 for {data.cameraState}");
                vJoy.getController().QuickClick(114u);
            } else
                _logger.LogWarning($"Wrong camera state for chase view toggle {data.cameraState}");
        }
    }
}
