using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BaroData
    {
        [SCStructField("BAROMETER PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float barometerPressureMB;
        [SCStructField("SEA LEVEL PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float seaLevelPressureMB;
        [SCStructField("KOHLSMAN SETTING MB", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB; // This is the only settable one, via "KOHLSMAN_SET".
        [SCStructField("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanHg;
    };

    [Singleton]
    public class BaroListener : DataListener<BaroData>, IRequestDataOnOpen
    {

        private readonly DebugConsole dc;

        public BaroListener(DebugConsole dc)
        {
            this.dc = dc;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(SimConnect _, BaroData data)
        {
            dc.Text = $"baro {data.barometerPressureMB}MB MSL {data.seaLevelPressureMB}MB"
                + $"\nKohlsman {data.kohlsmanMB}MB/{data.kohlsmanHg}Hg";
        }

    }

}
