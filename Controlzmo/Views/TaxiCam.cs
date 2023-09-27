using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;
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
        // "EYEPOINT POSITION" as a SIMCONNECT_DATA_XYZ? Do you have to use that instead of our own struct?
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class TaxiCam : DataListener<TaxiCamData>
    {
        private readonly ILogger<TaxiCam> _log;

        public override void Process(ExtendedSimConnect simConnect, TaxiCamData data)
        {
            _log.LogCritical($"**--** taxi cam! '{data.title}'");
            simConnect.CameraSetRelative6DOF(0.525f, -2f, -25f, 15f, 0f, 0f);
        }
    }
}
