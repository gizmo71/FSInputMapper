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
        public float radioAlt;
        [SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onGround;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingApproachRateTrigger : DataListener<LandingApproachRateTriggerData>, IRequestDataOnOpen
    {
        private readonly LandingApproachRate rateListener;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private SIMCONNECT_PERIOD current = SIMCONNECT_PERIOD.NEVER;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, LandingApproachRateTriggerData data)
        {
            var period = data.radioAlt > 50.0 && data.onGround == 1 ? SIMCONNECT_PERIOD.NEVER : SIMCONNECT_PERIOD.SECOND;
            if (period != current)
            {
                simConnect.RequestDataOnSimObject(rateListener, current = period);
                if (period == SIMCONNECT_PERIOD.NEVER)
                    hub.Clients.All.UpdateLandingRate(null, null);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingApproachRateData
    {
        [SimVar("VERTICAL SPEED", "Feet per minute", SIMCONNECT_DATATYPE.FLOAT32, 10.0f)]
        public float feetPerSecond;
        [SimVar("PLANE ALT ABOVE GROUND MINUS CG", "feet", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 radioAlt;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingApproachRate : DataListener<LandingApproachRateData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        [Property]
        private int correction = 0;
        public override void Process(ExtendedSimConnect simConnect, LandingApproachRateData data)
        {
            hub.Clients.All.UpdateLandingRate((int) (data.feetPerSecond * 1), data.radioAlt - correction);
        }
    }
}
