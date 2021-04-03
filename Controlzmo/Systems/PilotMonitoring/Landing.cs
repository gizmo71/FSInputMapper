using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    /* Would be good to detect reversers for "Rev Green"
     * How? A32NX_AUTOTHRUST_REVERSE:1/2?
     * TURB ENG REVERSE NOZZLE PERCENT:index?
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingData
    {
        [SimVar("ACCELERATION BODY Z", "feet per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.25f)]
        public double accelerationZ;
        [SimVar("SPOILERS LEFT POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT32, 0.05f)]
        public float spoilersLeft;
        [SimVar("SPOILERS RIGHT POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT32, 0.05f)]
        public float spoilersRight;
    };

    [Component]
    public class LandingListener : DataListener<LandingData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly LocalVarsListener localVarsListener;

        private bool? wasDecel = null;
        private bool? wasSpoilers = null;

        public LandingListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            localVarsListener = serviceProvider.GetRequiredService<LocalVarsListener>();
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }

        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            simConnect.RequestDataOnSimObject(this, isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER);
            wasDecel = wasSpoilers = isOnGround ? false : null;
        }

        public override void Process(ExtendedSimConnect simConnect, LandingData data)
        {
System.Console.Error.WriteLine($"Decel: was {wasDecel} rate {data.accelerationZ}");
//TODO: have a minimum speed before decel listener is armed
            if (wasDecel == false /*&& localVarsListener.localVars.autobraking == 1*/)
            {
                var requiredDecel = localVarsListener.localVars.autobrake * -2;
                bool isDecel = requiredDecel != 0 && requiredDecel > data.accelerationZ;
                if (isDecel)
                {
                    hubContext.Clients.All.Speak("Decell!");
                    wasDecel = true;
                }
            }

System.Console.Error.WriteLine($"Spoilers: was {wasSpoilers} left {data.spoilersLeft} right {data.spoilersRight}");
            if (wasSpoilers == false)
            {
                bool isSpoilers = data.spoilersLeft > .675 && data.spoilersRight > .675;
                if (isSpoilers)
                {
                    hubContext.Clients.All.Speak("Spoilers!");
                    wasSpoilers = true;
                }
            }
        }
    }
}
