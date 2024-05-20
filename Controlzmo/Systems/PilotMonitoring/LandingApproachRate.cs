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
        [SimVar("PLANE ALT ABOVE GROUND", "feet", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public float radioAlt;
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
            var period = data.radioAlt > 50.0 && data.radioAlt < 0.1 ? SIMCONNECT_PERIOD.NEVER : SIMCONNECT_PERIOD.SECOND;
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
        [SimVar("VELOCITY BODY Y", "Feet per minute", SIMCONNECT_DATATYPE.INT32, 10.0f)]
        public Int32 feetPerMinute;
        [SimVar("PLANE ALT ABOVE GROUND", "feet", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 radioAlt;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingApproachRate : DataListener<LandingApproachRateData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        public override void Process(ExtendedSimConnect simConnect, LandingApproachRateData data)
        {
            hub.Clients.All.UpdateLandingRate(data.feetPerMinute, data.radioAlt);
        }
    }
}
