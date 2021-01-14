using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LightData
    {
        [SCStructField("LIGHT POTENTIOMETER:7", "Number", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float domeLight; // 0=off, 0.5=dim, 1.0=bright
    };
}
