using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    // FCU - general set of things we receive.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApData
    {
        [DataField("AUTOPILOT AIRSPEED HOLD VAR", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.5f)]
        public double speedKnots; // Real range 100 -399 knots (Mach 0.10-0.99).
        [DataField("AUTOPILOT SPEED SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 speedSlot;
        // Correct for selected, but not writable. When the user is pre-selecting, remains on the managed number.
        [DataField("AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 heading; // Real range 000-359 (not 360!)
        [DataField("AUTOPILOT HEADING SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 headingSlot;
        // In selected mode, this is correct (but not writable).
        // In managed mode, it shows what the autopilot is really doing (which may be modified by constraints).
        // Have not yet found where the displayed panel value is (may not be available via SimConnect).
        [DataField("AUTOPILOT ALTITUDE LOCK VAR", "feet", SIMCONNECT_DATATYPE.INT32, 50f)]
        public Int32 altitude; // Real range 100-49000
        [DataField("AUTOPILOT ALTITUDE SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 altitudeSlot;
        [DataField("AUTOPILOT VERTICAL HOLD VAR", "Feet/minute", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vs;
        [DataField("AUTOPILOT VS SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
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
        [DataField("AUTOPILOT FLIGHT DIRECTOR ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fdActive;
        [DataField("AUTOPILOT MASTER", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apMaster;
        [DataField("AUTOPILOT HEADING LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apHeadingHold;
        [DataField("AUTOPILOT APPROACH HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 approachHold;
        [DataField("AUTOPILOT NAV1 LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 nav1Hold;
        [DataField("AUTOPILOT GLIDESLOPE HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 gsHold;
        [DataField("AUTOPILOT ALTITUDE LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apAltHold;
        [DataField("AUTOPILOT VERTICAL HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apVSHold;
        [DataField("AUTOPILOT THROTTLE ARM", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 autothrustArmed;
        [DataField("AUTOTHROTTLE ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 autothrustActive;
        //TODO: EXPED button, when it's implemented
    }

    // FCU - things we get when pulling Heading to Selected.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApHdgSelData
    {
        [DataField("PLANE HEADING DEGREES MAGNETIC", "degrees", SIMCONNECT_DATATYPE.INT32, 0f)]
        public UInt32 headingMagnetic;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerData
    {
        [DataField("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersHandlePosition;
        [DataField("SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersArmed;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerHandle
    {
        [DataField("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersHandlePosition;
    };

}
