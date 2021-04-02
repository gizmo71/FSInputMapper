using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    /*On landing, would be good to detect reversers (how? A32NX_AUTOTHRUST_REVERSE:1/2?)*/
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

        private bool wasDecel = false;
        private bool wasSpoilers = false;

        public LandingListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            localVarsListener = serviceProvider.GetRequiredService<LocalVarsListener>();
        }

        public override void Process(ExtendedSimConnect simConnect, LandingData data)
        {
System.Console.Error.WriteLine($"Decel: was {wasDecel} rate {data.accelerationZ}"
    + $"\n\tSpoilers left {data.spoilersLeft} right {data.spoilersRight}");
            bool isDecel = localVarsListener.localVars.autobrake > 0 && (-3) > data.accelerationZ;
//TODO: need minimum speed too - otherwise we get it with all braking. Perhaps it should only be said once after ground latches on?
            if (!wasDecel && isDecel)
                hubContext.Clients.All.Speak("decell");
            wasDecel = isDecel;

            bool isSpoilers = data.spoilersLeft > .675 && data.spoilersRight > .675;
            if (!wasSpoilers && isSpoilers)
                hubContext.Clients.All.Speak("spoilers");
            wasSpoilers = isSpoilers;
        }
    }
}
