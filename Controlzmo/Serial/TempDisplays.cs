using Controlzmo.Hubs;
using Controlzmo.Systems.FlightControlUnit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
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

    [Component]
    public class FcuDisplayRight : CreateOnStartup
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly FcuAltManaged fcuAltManaged;

        public FcuDisplayRight(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            fcuAltManaged = sp.GetRequiredService<FcuAltManaged>();
            fcuAltManaged.PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var managed = fcuAltManaged.IsManaged ? "*" : " ";
            string line1, line2;
            line1 = "ALT ┌LVL/CH┐ " + (true ? "V/S" : "FPA");
            line2 = $"{12345:00000}   {managed}  {666:+0000}";
            //serial.SendLine($"fcuDisplayRight1={line1}");
            //serial.SendLine($"fcuDisplayRight2={line2}");
            hub.Clients.All.SetFromSim("fcuDisplayRight", $"{line1}\n{line2}");
        }
    }
}
