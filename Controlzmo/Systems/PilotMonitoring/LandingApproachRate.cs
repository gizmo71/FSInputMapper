using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingApproachRateTriggerData
    {
        [SimVar("PLANE ALT ABOVE GROUND MINUS CG", "feet", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public Int32 radioAlt;
        [SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onGround;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingApproachRateTrigger : DataListener<LandingApproachRateTriggerData>, IRequestDataOnOpen
    {
        private readonly LandingApproachRate rateListener;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private SIMCONNECT_PERIOD current = SIMCONNECT_PERIOD.SECOND;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, LandingApproachRateTriggerData data)
        {
            var period = (data.radioAlt > 400 || data.onGround == 1) ? SIMCONNECT_PERIOD.NEVER : SIMCONNECT_PERIOD.VISUAL_FRAME;
            if (period != current)
            {
                simConnect.RequestDataOnSimObject(rateListener, current = period);
                if (period == SIMCONNECT_PERIOD.NEVER)
                    hub.Clients.All.UpdateLandingRate(null, null, "white");
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingApproachRateData
    {
        [SimVar("VERTICAL SPEED", "Feet per minute", SIMCONNECT_DATATYPE.INT32, 10.0f)]
        public Int32 feetPerMinute;
        [SimVar("PLANE ALT ABOVE GROUND MINUS CG", "feet", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 radioAlt;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingApproachRate : DataListener<LandingApproachRateData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        public override void Process(ExtendedSimConnect simConnect, LandingApproachRateData data)
        {
            var colour = "yellow";
            if (data.feetPerMinute < -500)
                colour = "red";
            else if (data.feetPerMinute < -350)
                colour = "olive";
            else if (data.feetPerMinute < -150)
                colour = "green";
            else if (data.feetPerMinute < 0)
                colour = "greenyellow";
            else if (data.feetPerMinute >= 0)
                colour = "cyan";
            hub.Clients.All.UpdateLandingRate(data.feetPerMinute, data.radioAlt, colour);
        }
    }
}
