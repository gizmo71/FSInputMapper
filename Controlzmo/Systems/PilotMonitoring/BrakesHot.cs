using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [Component]
    public class BrakesHot : LVar, IOnSimConnection
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public BrakesHot(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        protected override string LVarName() => "A32NX_BRAKES_HOT";
        protected override int Milliseconds() => 4000;
        protected override double Default() => 1.0;

        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value
        {
            set { if ((base.Value = value) == 1.0) hubContext.Clients.All.Speak("Brakes hot"); }
        }
    }
}
