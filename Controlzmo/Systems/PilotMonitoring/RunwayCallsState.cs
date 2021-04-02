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
        [SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onGround;
        [SimVar("ON ANY RUNWAY", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onRunway;
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
System.Console.Error.WriteLine($"Runway calls state: {data.onGround}/{data.onRunway}");
            var period = data.onRunway == 1 ? SIMCONNECT_PERIOD.VISUAL_FRAME : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(takeOffListener, period);
            simConnect.RequestDataOnSimObject(landingListener, period);
        }
    }
}
