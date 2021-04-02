using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly TakeOffListener takeOffListener;
        private readonly LandingListener landingListener;

        public RunwayCallsStateListener(IServiceProvider serviceProvider)
        {
            takeOffListener = serviceProvider.GetRequiredService<TakeOffListener>();
            landingListener = serviceProvider.GetRequiredService<LandingListener>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, RunwayCallsStateData data)
        {
System.Console.Error.WriteLine($"Runway calls state: {data.onGround}");
            var period = data.onGround == 1 ? SIMCONNECT_PERIOD.VISUAL_FRAME : SIMCONNECT_PERIOD.NEVER;
//TODO: tell each listener the state, so that it can make adjustments
// e.g. TO one can set all flags true once airbourne, regardleess of whether calls made.
// Landing one can set all flags false once airbourne, and latch decel on when back on ground.
            simConnect.RequestDataOnSimObject(takeOffListener, period);
            simConnect.RequestDataOnSimObject(landingListener, period);
        }
    }
}
