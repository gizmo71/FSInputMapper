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
            var speedMachLabel = fcuSpeedMachListener.IsMach ? " MACH" : "SPD  ";
            var hdgTrkLabel = fcuTrackFpa.IsHdgVS ? " HDG  " : "   TRK";
            var line1 = $"{speedMachLabel}  {hdgTrkLabel} LAT ";

            var speedDot = fcuSpeedManaged.IsManaged ? '*' : ' ';
            var heading = 666;
            var headingDot = '?';
            var line2 = $"{Speed} {speedDot}   {heading}  {headingDot}  ";

            hub.Clients.All.SetFromSim("fcuDisplayLeft", $"{line1}\n{line2}");
        }

        private string Speed
        {
            get
            {
                string speed;
                var selection = (double)fcuSpeedSelection!;
                if (selection >= 0.10 && selection < 1.0)
                    speed = $"{selection:0.00}";
                else if (selection >= 100 && selection < 1000)
                    speed = $" {selection:000}";
                else
                    speed = " ---";
                return speed;
            }
        }
    }
}
