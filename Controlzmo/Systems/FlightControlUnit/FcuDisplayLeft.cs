using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace Controlzmo.Serial
{
    [Component]
    public class FcuDisplayLeft : CreateOnStartup
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public FcuDisplayLeft(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }
    }
}
