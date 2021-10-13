using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

namespace Controlzmo.Serial
{
    [Component]
    public class FcuDisplayLeft : ISettable<string>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public FcuDisplayLeft(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public string GetId() => "fcuDisplayLeft";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            hub.Clients.All.SetFromSim(GetId(), value);
        }
    }

    [Component]
    public class FcuDisplayRight : ISettable<string>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public FcuDisplayRight(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public string GetId() => "fcuDisplayRight";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            hub.Clients.All.SetFromSim(GetId(), value);
        }
    }
}
