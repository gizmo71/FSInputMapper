using System;
using System.Runtime.InteropServices;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Lights
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LightData
    {
        [SCStructField("LIGHT STROBE", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int strobeSwitch; // "Auto" comes back as on. :-(
        [SCStructField("LIGHT STROBE ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int strobeState; // "Auto" comes back as on. :-(
        [SCStructField("LIGHT BEACON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int beaconSwitch;
        [SCStructField("LIGHT BEACON ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int beaconState;
        [SCStructField("LIGHT WING", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int wingSwitch;
        [SCStructField("LIGHT WING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int wingState;
        [SCStructField("LIGHT NAV", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int navSwitch;
        [SCStructField("LIGHT NAV ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int navState; // Locked to logo
        [SCStructField("LIGHT LOGO", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int logoSwitch;
        [SCStructField("LIGHT LOGO ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int logoState; // Locked to nav
        //[SCStructField("LIGHT ? ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        //public Int32 runway;
        [SCStructField("LIGHT LANDING", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int landingSwitch;
        [SCStructField("LIGHT LANDING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int landingState;
        [SCStructField("LIGHT TAXI", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int noseSwitch;
        [SCStructField("LIGHT TAXI ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int noseState;
        [SCStructField("LIGHT STATES", "Number", SIMCONNECT_DATATYPE.INT32, 0f)]
        public int mask; // nose=TO|landing 4, nose=taxi|runwayTurn 8
    };

    [Singleton]
    public class Listeners : DataListener<LightData>, IRequestDataOnOpen
    {

        private readonly LightSystem lightSystem;
        private readonly DebugConsole debugConsole;

        public Listeners(LightSystem lightSystem, DebugConsole debugConsole)
        {
            this.lightSystem = lightSystem;
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
    + $"\nMask {Convert.ToString(lightData.mask, 2).PadLeft(10, '0')}"
    + $"Strobes/Switch {lightData.strobeState}/{lightData.strobeSwitch}";
            lightSystem.Beacon = lightData.beaconSwitch == 1;
            lightSystem.Wing = lightData.wingSwitch == 1;
            lightSystem.NavLogo = (lightData.navState | lightData.logoState) != 0;
            lightSystem.RunwayTurnoff = lightData.noseState == 1;
            lightSystem.Landing = lightData.landingState == 1;
            lightSystem.Taxi = lightData.noseState == 1;
            lightSystem.Strobes = lightData.strobeState == 1;
        }

    }

}
