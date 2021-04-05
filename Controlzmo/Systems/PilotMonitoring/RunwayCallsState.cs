using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RunwayCallsStateData
    {
        // Would like to use "ON ANY RUNWAY" but it's just not reliable. :-(
        [SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onGround;
    };

    [Component]
    public class RunwayCallsStateListener : DataListener<RunwayCallsStateData>, IRequestDataOnOpen
    {
        public delegate void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround);
        public event OnGroundHandler? onGroundHandlers;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, RunwayCallsStateData data)
        {
//System.Console.Error.WriteLine($"Runway calls state: {data.onGround}");
            var period = data.onGround == 1 ? SIMCONNECT_PERIOD.VISUAL_FRAME : SIMCONNECT_PERIOD.NEVER;
            onGroundHandlers?.Invoke(simConnect, data.onGround == 1);
        }
    }
}
