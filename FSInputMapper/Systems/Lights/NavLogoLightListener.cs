using System;
using System.Runtime.InteropServices;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Lights
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct NavLogoLightData
    {
        [SCStructField("LIGHT NAV", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int navSwitch;
        [SCStructField("LIGHT NAV ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int navState; // Locked to logo
        [SCStructField("LIGHT LOGO", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int logoSwitch;
        [SCStructField("LIGHT LOGO ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int logoState; // Locked to nav
    }

    [Singleton]
    public class NavLogoLightListener : DataListener<NavLogoLightData>, IRequestDataOnOpen
    {

        private readonly LightSystem lightSystem;

        public NavLogoLightListener(LightSystem lightSystem)
        {
            this.lightSystem = lightSystem;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.SECOND;
        }

        public override void Process(SimConnect simConnect, NavLogoLightData data)
        {
            if (data.logoSwitch != data.navSwitch)
                lightSystem.SetNavLogo(true);
            lightSystem.NavLogo = data.navSwitch + data.logoSwitch != 0;
        }

    }

}
