using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct OnRunwayStateData
    {
        // Literally on the runway; goes on during line up.
        // Comes off a couple of seconds after lift off - is it because we drifted off the runway after lift off?
        [SimVar("ON ANY RUNWAY", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onAnyRunway;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class OnRunwayStateListener : DataListener<OnRunwayStateData>, IRequestDataOnOpen
    {
        //private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, OnRunwayStateData data)
        {
            //hubContext.Clients.All.Speak((data.onAnyRunway == 1 ? "" : "not ") + "on any runway");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GroundCallsStateData
    {
        [SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onGround;
    };

    [Component]
    public class RunwayCallsStateListener : DataListener<GroundCallsStateData>, IRequestDataOnOpen
    {
        public delegate void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround);
        public event OnGroundHandler? onGroundHandlers;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, GroundCallsStateData data)
        {
            var period = data.onGround == 1 ? SIMCONNECT_PERIOD.VISUAL_FRAME : SIMCONNECT_PERIOD.NEVER;
            onGroundHandlers?.Invoke(simConnect, data.onGround == 1);
        }
    }
}
