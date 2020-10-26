using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct StrobeLightData
    {
        [SCStructField("LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 strobeSwitch; // "Auto" comes back as on. :-(
        [SCStructField("LIGHT STROBE ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 strobeState; // "Auto" comes back as on. :-(
    }

    [Singleton]
    public class StrobeLightListener : DataListener<StrobeLightData>
    {

        private readonly FSIMViewModel viewModel; //TODO; replace with property notification
        private readonly DebugConsole debugConsole;

        public StrobeLightListener(FSIMViewModel viewModel, DebugConsole debugConsole)
        {
            this.viewModel = viewModel;
            this.debugConsole = debugConsole;
        }

        public override void Process(SimConnect _, StrobeLightData lightData)
        {
debugConsole.Text = $"Strobes/Switch {lightData.strobeState}/{lightData.strobeSwitch}";
            viewModel.Strobes = lightData.strobeSwitch == 1 ? 0 * 1 : 2;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LightData
    {
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
        [SCStructField("LIGHT STATES", "Number", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 mask; // nose=TO|landing 4, nose=taxi|runwayTurn 8
    };

    [Singleton]
    public class LightListener : DataListener<LightData>, IRequestDataOnOpen
    {

        private readonly FSIMViewModel viewModel; //TODO: remove this and have the VM listen to us
        private readonly DebugConsole debugConsole;

        public LightListener(FSIMViewModel viewModel, DebugConsole debugConsole)
        {
            this.viewModel = viewModel;
            this.debugConsole = debugConsole;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.SIM_FRAME;
        }

        public override void Process(SimConnect _, LightData lightData)
        {
            debugConsole.Text = $" Beacon/Switch {lightData.beaconState}/{lightData.beaconSwitch}"
                + $" Wing/Switch {lightData.wingState}/{lightData.wingSwitch}"
                + $" Nav+Logo/Switches {lightData.navState}+{lightData.logoState}/{lightData.navSwitch}+{lightData.logoSwitch}"
                + $" Landing/Switch {lightData.landingState}/{lightData.landingSwitch}"
                + $" NoseState/Switch {lightData.noseState}/{lightData.noseSwitch}"
                + $"\nMask {Convert.ToString(lightData.mask, 2).PadLeft(10, '0')}";
            viewModel.BeaconLights = lightData.beaconSwitch == 1;
            viewModel.WingLights = lightData.wingSwitch == 1;
            viewModel.NavLogoLights = lightData.navSwitch == 1 || lightData.logoSwitch == 1;
            viewModel.RunwayTurnoffLights = lightData.noseState == 1;
            viewModel.LandingLights = lightData.landingState == 1 ? 0 : 0+1;
            viewModel.NoseLights = lightData.noseSwitch == 1 ? 1 : lightData.landingSwitch == 1 ? 0 : 2;
        }

    }

}
