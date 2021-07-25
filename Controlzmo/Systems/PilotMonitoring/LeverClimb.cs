using System;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [Component]
    public class LeverClimb : LVar, IOnSimConnection
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public LeverClimb(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        protected override string LVarName() => "A32NX_AUTOTHRUST_MODE_MESSAGE";
        protected override int Milliseconds() => 1000;
        protected override double Default() => 1.0;

        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value
        {
            set { if ((base.Value = value) == 3.0) hubContext.Clients.All.Speak("lever climb"); }
        }
    }
}
