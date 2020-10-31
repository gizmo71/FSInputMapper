﻿using System;
using System.Runtime.InteropServices;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Lights
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingLightData
    {
        // No way to detect "Off" position between On and Retract.
        [SCStructField("LIGHT LANDING", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingSwitch;
        [SCStructField("LIGHT LANDING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingState;
    }

    [Singleton]
    public class LandingLightListener : DataListener<LandingLightData>, IRequestDataOnOpen
    {

        private readonly LightSystem lightSystem;

        public LandingLightListener(LightSystem lightSystem)
        {
            this.lightSystem = lightSystem;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.SECOND;
        }

        public override void Process(SimConnect simConnect, LandingLightData data)
        {
            lightSystem.Landing = data.landingState == 1;
        }

    }

}