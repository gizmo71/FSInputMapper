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
            _log.LogCritical($"**--** taxi cam! '{data.title}' for '{simConnect.AircraftFile}'");
// x/y/z, p/b/h
// x: negative left, positive right
// y: positive above, negative below
// z: positive is ahead, negative is behind
// p: positive is down, negative is up
// b: negative anticlockwise, positive clockwise
// h: 0 is forward, -90 left, 90 right
            var y = -2f;
            var z = -25f;
            if ("aircrafta321neopw" == simConnect.AircraftFile)
            {
                y = -2.4f;
                z = -28f;
            }
            simConnect.CameraSetRelative6DOF(0.525f, y, z, 15f, 0f, 0f);
        }
    }
}
