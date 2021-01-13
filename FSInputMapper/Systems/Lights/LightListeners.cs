using System.Runtime.InteropServices;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Lights
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LightData
    {
        [SCStructField("LIGHT WING", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int wingSwitch;
        [SCStructField("LIGHT WING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int wingState;
        [SCStructField("LIGHT TAXI", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseSwitch;
        [SCStructField("LIGHT TAXI:2", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseSwitch2; // runway turn off (left?)
        [SCStructField("LIGHT TAXI:3", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseSwitch3; // runway turn off (right?)
        [SCStructField("LIGHT TAXI ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseState;
        [SCStructField("LIGHT POTENTIOMETER:7", "Number", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float domeLight; // 0=off, 0.5=dim, 1.0=bright
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
            lightSystem.Wing = lightData.wingSwitch == 1;
            lightSystem.RunwayTurnoff = lightData.noseSwitch == 1;
        }

    }

}
