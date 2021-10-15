using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuDisplayLeft : CreateOnStartup
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly FcuSpeedMachListener fcuSpeedMachListener;
        private readonly FcuSpeedManaged fcuSpeedManaged;
        private readonly FcuSpeedSelection fcuSpeedSelection;
        private readonly FcuTrackFpa fcuTrackFpa;

        public FcuDisplayLeft(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            (fcuSpeedMachListener = sp.GetRequiredService<FcuSpeedMachListener>()).PropertyChanged += Regenerate;
            (fcuSpeedManaged = sp.GetRequiredService<FcuSpeedManaged>()).PropertyChanged += Regenerate;
            (fcuSpeedSelection = sp.GetRequiredService<FcuSpeedSelection>()).PropertyChanged += Regenerate;
            (fcuTrackFpa = sp.GetRequiredService<FcuTrackFpa>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var line1 = (fcuSpeedMachListener.IsMach ? "  MACH" : "SPD   ")
                + (fcuTrackFpa.IsHdgVS ? " HDG " : "  TRK")
                + " LAT ";
            var speedDot = fcuSpeedManaged.IsManaged ? '*' : ' ';
            var heading = 666;
            var headingDot = '?';
            var line2 = $" {Speed} {speedDot}  {heading}  {headingDot}  ";

            hub.Clients.All.SetFromSim("fcuDisplayLeft", $"{line1}\n{line2}");
        }

        private string Speed
        {
            get
            {
                string speed;
                if (fcuSpeedSelection.IsDashes)
                    speed = "--- ";
                else if (fcuSpeedMachListener.IsMach)
                    speed = $"{(double)fcuSpeedSelection!:0.00}";
                else
                    speed = $"{(double)fcuSpeedSelection!:000} ";
                return speed;
            }
        }
    }
}
