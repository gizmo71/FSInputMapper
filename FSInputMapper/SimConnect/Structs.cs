﻿using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

//TODO: split the listeners up and move these to each
namespace FSInputMapper
{

    // FCU - general set of things we receive.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApData
    {
        // In selected mode, this is correct (but not writable).
        // In managed mode, it shows what the autopilot is really doing (which may be modified by constraints).
        // Have not yet found where the displayed panel value is (may not be available via SimConnect).
        [SCStructField("AUTOPILOT ALTITUDE LOCK VAR", "feet", SIMCONNECT_DATATYPE.INT32, 50f)]
        public Int32 constrainedAltitude; // Real range 100-49000
        [SCStructField("AUTOPILOT ALTITUDE LOCK VAR:3", "feet", SIMCONNECT_DATATYPE.INT32, 50f)]
        public Int32 selectedAltitude; // Real range 100-49000
        [SCStructField("AUTOPILOT ALTITUDE SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 altitudeSlot;
        [SCStructField("AUTOPILOT VERTICAL HOLD VAR", "Feet/minute", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vs;
        [SCStructField("AUTOPILOT VS SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vsSlot;
        //TODO: set V/S to 0 on push, and engage selected V/S on pull; after 0ing, subsequent turns are actioned immediately
        //TODO: V/S selector; real range ±6000ft/min in steps of 100, or ±9.9º in steps of 0.1º
        //TODO: SPD/MCH buton; is it implemented yet?
        //TODO: HDG-V/S TRK-FPA toggle, when it's implemented
        //TODO: metric alt - does this work in MSFS?
    };

    // Autopilot - things we receive.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApModeData
    {
        [SCStructField("AUTOPILOT FLIGHT DIRECTOR ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fdActive;
        [SCStructField("AUTOPILOT MASTER", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apMaster;
        [SCStructField("AUTOPILOT HEADING LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apHeadingHold;
        [SCStructField("AUTOPILOT APPROACH HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 approachHold;
        [SCStructField("AUTOPILOT NAV1 LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 nav1Hold;
        [SCStructField("AUTOPILOT GLIDESLOPE HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 gsHold;
        [SCStructField("AUTOPILOT ALTITUDE LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apAltHold;
        [SCStructField("AUTOPILOT VERTICAL HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apVSHold;
        //TODO: EXPED button, when it's implemented
    };

}
