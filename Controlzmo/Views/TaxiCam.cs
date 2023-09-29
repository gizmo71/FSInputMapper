using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Lombok.NET;

namespace Controlzmo.Views
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TaxiCamData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [SimVar("TITLE", null, SIMCONNECT_DATATYPE.STRING128, 0.5f)]
        public string title;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class TaxiCam : DataListener<TaxiCamData>
    {
        private readonly ILogger<TaxiCam> _log;

        public override void Process(ExtendedSimConnect simConnect, TaxiCamData data)
        {
            _log.LogCritical($"**--** taxi cam! '{data.title}' for '{simConnect.AircraftFile}'");
            simConnect.SendDataOnSimObject(new CameraVariableData() { cameraState = 5, cameraSubState = 0, viewType = 5, viewIndex = 3 });
        }
    }
}
