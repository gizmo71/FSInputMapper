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
    public struct DecelData
    {
        [SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onGround;
        [SimVar("AUTO BRAKE SWITCH CB", "Enum", SIMCONNECT_DATATYPE.FLOAT32, 0.01f)]
        public float autoBrakeSwitch; // 0 for any on and 1 for off WTF?!
        [SimVar("ACCELERATION BODY Z", "feet per second squared", SIMCONNECT_DATATYPE.FLOAT64, 1.5f)]
        public double accelerationZ;
    };

    [Component]
    public class DecelListener : DataListener<DecelData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private bool wasDecel = false;

        public DecelListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, DecelData data)
        {
System.Console.Error.WriteLine($"Decel: was {wasDecel} - now onGround {data.onGround} autoBrakeSwitch {data.autoBrakeSwitch} rate {data.accelerationZ}");
            bool isDecel = data.onGround == 1
                && data.autoBrakeSwitch != 1
                && (-3) > data.accelerationZ;
            if (!wasDecel && isDecel)
                hubContext.Clients.All.Speak("decel");
            wasDecel = isDecel;
        }
    }
}
