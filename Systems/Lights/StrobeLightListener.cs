using System.Runtime.InteropServices;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Lights
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct StrobeLightData
    {
        // "Auto" comes back as on. :-(
        [SCStructField("LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public int strobeSwitch;
        [SCStructField("LIGHT STROBE ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public int strobeState;
        [SCStructField("LIGHT POTENTIOMETER:24", "Number", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public int lPot24;
    }

    [Singleton]
    public class StrobeLightListener : DataListener<StrobeLightData>, IRequestDataOnOpen
    {

        private readonly LightSystem lightSystem;

        public StrobeLightListener(LightSystem lightSystem)
        {
            this.lightSystem = lightSystem;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(SimConnect simConnect, StrobeLightData data)
        {
            lightSystem.Strobes = data.strobeSwitch == 1;
            lightSystem.IsStrobeAuto = data.lPot24 == 0;
        }

    }

}
