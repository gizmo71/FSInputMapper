using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;
using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

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

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, LandingApproachRateTriggerData data)
        {
            var period = SIMCONNECT_PERIOD.SECOND;
            if (data.radioAlt > 50.0 && data.radioAlt < 0.1)
            {
                 period = SIMCONNECT_PERIOD.NEVER;
                hub.Clients.All.UpdateLandingRate(null);
            }
            simConnect.RequestDataOnSimObject(rateListener, period);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingApproachRateData
    {
        [SimVar("VELOCITY BODY Y", "Feet per minute", SIMCONNECT_DATATYPE.INT32, 10.0f)]
        public Int32 feetPerMinute;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingApproachRate : DataListener<LandingApproachRateData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        public override void Process(ExtendedSimConnect simConnect, LandingApproachRateData data)
        {
            hub.Clients.All.UpdateLandingRate(data.feetPerMinute);
        }
    }
}
