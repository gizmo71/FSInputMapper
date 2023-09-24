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
        protected readonly ILogger _logger;

        public CockpitExternalToggle(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<CockpitExternalToggle>>();
        }

        public int GetButton() => T16000mHotas.BUTTON_SIDE_RED;
        public void OnPress(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);

        public override void Process(ExtendedSimConnect simConnect, CockpitExternalToggleData data)
        {
            if (data.cameraState == 2) // If cockpit...
                data.cameraState = 3; // ... go chase
            else if (data.cameraState == 3 || data.cameraState == 4) // If chase or drone...
                data.cameraState = 2; // ... go cockpit...
            else
                return;
            simConnect.SendDataOnSimObject(data);
        }
    }
}
