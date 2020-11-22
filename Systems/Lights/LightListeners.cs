using System.Runtime.InteropServices;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Lights
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LightData
    {
        [SCStructField("LIGHT BEACON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int beaconSwitch;
        [SCStructField("LIGHT BEACON ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int beaconState;
        [SCStructField("LIGHT WING", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int wingSwitch;
        [SCStructField("LIGHT WING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int wingState;
        [SCStructField("LIGHT TAXI", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseSwitch;
        [SCStructField("LIGHT TAXI ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseState;
    };

    [Singleton]
    public class LightListener : DataListener<LightData>, IRequestDataOnOpen
    {

        private readonly LightSystem lightSystem;

        public LightListener(LightSystem lightSystem)
        {
            this.lightSystem = lightSystem;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(SimConnect _, LightData lightData)
        {
            lightSystem.Beacon = lightData.beaconSwitch == 1;
            lightSystem.Wing = lightData.wingSwitch == 1;
            lightSystem.RunwayTurnoff = lightData.noseSwitch == 1;
        }

    }

}
