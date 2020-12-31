using System;
using System.Runtime.InteropServices;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ComData
    {
        [SCStructField("COM ACTIVE FREQUENCY", "BCD16", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 activeFrequency;
        [SCStructField("COM STANDBY FREQUENCY", "BCD16", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standbyFrequency;
    };

    [Singleton]
    public class ComRadioListener : DataListener<ComData>, IRequestDataOnOpen
    {

        private readonly DebugConsole dc;

        public ComRadioListener(DebugConsole dc)
        {
            this.dc = dc;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(SimConnect _, ComData data)
        {
            dc.Text = String.Format("Active {0:X09}\nStandby {1:X09}", data.activeFrequency, data.standbyFrequency);
        }

    }

}
