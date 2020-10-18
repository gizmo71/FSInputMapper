using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    //http://www.prepar3d.com/SDKv3/LearningCenter/utilities/variables/simulation_variables.html#Aircraft%20Lights%20Variables
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LightData
    {
        [SCStructField("LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 strobeSwich; // "Auto" comes back as on. :-(
        [SCStructField("LIGHT STROBE ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 strobe; // "Auto" comes back as on. :-(
        [SCStructField("LIGHT BEACON ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 beacon;
        [SCStructField("LIGHT WING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 wing;
        [SCStructField("LIGHT NAV ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 nav; // Locked to logo
        [SCStructField("LIGHT LOGO ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 logo; // Locked to nav
        //[SCStructField("LIGHT ? ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        //public Int32 runway;
        [SCStructField("LIGHT LANDING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 landing;
        [SCStructField("LIGHT TAXI ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 nose; // Three pos? None work
        [SCStructField("LIGHT STATES", "Number", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 mask; // Nav 1, Beacon 2, nose=TO|either landing 4, nose=taxi|runwayTurn 8, Strobes 16 (still bool), wing 128, logo 256
        [SCStructField("LIGHT ON STATES", "Number", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 maskOn; // Nav 1, Beacon 2, Strobes 16, wing 128, logo 256
    };

    [Singleton]
    public class LightListener : DataListener<LightData>
    {

        private readonly FSIMViewModel viewModel;

        public LightListener(FSIMViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override void Process(SimConnectAdapter _, LightData lightData)
        {
            viewModel.Strobes = lightData.strobe != 1 ? 2 : 0;
            viewModel.GSToolTip = $"Strobes/Switch {lightData.strobe}/{lightData.strobeSwich} Beacon {lightData.beacon} Wing {lightData.wing} Nav/Logo {lightData.nav}/{lightData.logo}"
                + $"\nRunway ?? Landing {lightData.landing} Nose {lightData.nose}"
                + $"\nMask {Convert.ToString(lightData.mask, 2).PadLeft(10, '0')} Mask On {Convert.ToString(lightData.maskOn, 2).PadLeft(10, '0')}";
        }

    }

}
