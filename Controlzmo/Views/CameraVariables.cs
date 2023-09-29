using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;

namespace Controlzmo.Views
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CameraVariableData
    {
        [SimVar("CAMERA STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraState;
        [SimVar("CAMERA SUBSTATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraSubState;
        [SimVar("CAMERA VIEW TYPE AND INDEX:0", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 viewType;
        [SimVar("CAMERA VIEW TYPE AND INDEX:1", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 viewIndex;
    }

    [Component]
    public class CameraVariables : IData<CameraVariableData> { }
}
