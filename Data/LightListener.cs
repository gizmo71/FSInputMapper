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
        public Int32 strobeSwitch; // "Auto" comes back as on. :-(
        [SCStructField("LIGHT STROBE ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 strobeState; // "Auto" comes back as on. :-(
        [SCStructField("LIGHT BEACON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 beaconSwitch;
        [SCStructField("LIGHT BEACON ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 beaconState;
        [SCStructField("LIGHT WING", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 wingSwitch;
        [SCStructField("LIGHT WING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 wingState;
        [SCStructField("LIGHT NAV", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 navSwitch;
        [SCStructField("LIGHT NAV ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 navState; // Locked to logo
        [SCStructField("LIGHT LOGO", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 logoSwitch;
        [SCStructField("LIGHT LOGO ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 logoState; // Locked to nav
        //[SCStructField("LIGHT ? ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        //public Int32 runway;
        [SCStructField("LIGHT LANDING", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 landingSwitch;
        [SCStructField("LIGHT LANDING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 landingState;
        [SCStructField("LIGHT TAXI", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 noseSwitch;
        [SCStructField("LIGHT TAXI ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 noseState;
        [SCStructField("LIGHT RECOGNITION", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 recognitionSwitch;
        [SCStructField("LIGHT RECOGNITION ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 recognitionState;
        [SCStructField("LIGHT STATES", "Number", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 mask; // nose=TO|landing 4, nose=taxi|runwayTurn 8
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
            viewModel.DebugText = $"Strobes/Switch {lightData.strobeState}/{lightData.strobeSwitch}"
                + $" Beacon/Switch {lightData.beaconState}/{lightData.beaconSwitch}"
                + $" Wing/Switch {lightData.wingState}/{lightData.wingSwitch}"
                + $" Nav+Logo/Switches {lightData.navState}+{lightData.logoState}/{lightData.navSwitch}+{lightData.logoSwitch}"
                + $"\nRecog/Switch {lightData.recognitionState}/{lightData.recognitionSwitch}"
                + $"\nRunway ??"
                + $" Landing/Switch {lightData.landingState}/{lightData.landingSwitch}"
                + $" NoseState/Switch {lightData.noseState}/{lightData.noseSwitch}"
                + $"\nMask {Convert.ToString(lightData.mask, 2).PadLeft(10, '0')}";
            viewModel.Strobes = lightData.strobeSwitch == 1  ? 0*1 : 2;
            viewModel.BeaconLights = lightData.beaconSwitch == 1;
            viewModel.WingLights = lightData.wingSwitch == 1;
            viewModel.NavLogoLights = lightData.navSwitch == 1 || lightData.logoSwitch == 1;
            viewModel.RunwayTurnoffLights = lightData.noseState == 1;
            viewModel.LandingLights = lightData.landingState == 1 ? 0 : 0+1;
            viewModel.NoseLights = lightData.noseSwitch == 1 ? 1 : lightData.landingSwitch == 1 ? 0 : 2;
        }

    }

}
