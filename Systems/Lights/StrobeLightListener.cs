using System;
using System.Runtime.InteropServices;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Lights
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct StrobeLightData
    {
        // "Auto" comes back as on. :-(
        [SCStructField("LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int strobeSwitch;
        [SCStructField("LIGHT STROBE ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int strobeState;
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
            return SIMCONNECT_PERIOD.SECOND;
        }

        public override void Process(SimConnect simConnect, StrobeLightData data)
        {
            lightSystem.Strobes = data.strobeSwitch == 1;
        }

    }

}
