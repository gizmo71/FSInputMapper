using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingData
    {
        [SimVar("ACCELERATION BODY Z", "feet per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.25f)]
        public double accelerationZ;
        [SimVar("SPOILERS LEFT POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT32, 0.05f)]
        public float spoilersLeft;
        [SimVar("SPOILERS RIGHT POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT32, 0.05f)]
        public float spoilersRight;
        [SimVar("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 kias;
        [SimVar("TURB ENG REVERSE NOZZLE PERCENT:1", "percent", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 rev1;
        [SimVar("TURB ENG REVERSE NOZZLE PERCENT:2", "percent", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 rev2;
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
            SIMCONNECT_PERIOD period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            wasDecel = null;
            wasSpoilers = isOnGround ? false : null;
        }

        private const double MIN_SPOILER_DEPLOYMENT = .9;

        public override void Process(ExtendedSimConnect simConnect, LandingData data)
        {
System.Console.Error.WriteLine($"Decel: was {wasDecel} rate {data.accelerationZ} kias {data.kias}");
            if (wasDecel == null && data.kias >= 80)
            {
                wasDecel = false;
            }
            if (wasDecel == false)
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
                bool isSpoilers = data.spoilersLeft > MIN_SPOILER_DEPLOYMENT && data.spoilersRight > MIN_SPOILER_DEPLOYMENT;
                if (isSpoilers)
                {
                    hubContext.Clients.All.Speak("Spoilers!");
                    wasSpoilers = true;
                }
            }

System.Console.Error.WriteLine($"Reversers: one {data.rev1} two {data.rev2}");
            //TODO: "rev green"?
        }
    }
}
