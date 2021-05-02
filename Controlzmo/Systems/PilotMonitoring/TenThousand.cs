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
    public struct TenThousandData
    {
        // Alternatively "PLANE ALTITUDE"
        [SimVar("INDICATED ALTITUDE", "Feet", SIMCONNECT_DATATYPE.INT32, 50.0f)]
        public Int32 feetIndicated;
    };

    [Component]
    public class TenThousand : DataListener<TenThousandData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        Int32? previous = null;

        public TenThousand(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, TenThousandData data)
        {
            if (previous != null)
            {
                bool isAbove = data.feetIndicated > 10000;
                bool wasAbove = previous > 10000;
                if (isAbove != wasAbove)
                    hubContext.Clients.All.Speak("ten thousand");
            }
            previous = data.feetIndicated;
Console.WriteLine($"Now {previous} feet");
        }
    }
}
