using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Lights
{
    public class SeatBeltSign : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public SeatBeltSign(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "seatBeltSign";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            sender.Execute(simConnect, $"(A:CABIN SEATBELTS ALERT SWITCH,Bool) {desiredValue} =="
                + " if{{ (> K:CABIN_SEATBELTS_ALERT_SWITCH_TOGGLE) }}");
        }
    }
}
