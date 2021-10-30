using Controlzmo.Hubs;
using Controlzmo.Serial;
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
        private readonly SerialPico serial;
        private readonly FcuSpeedMachListener fcuSpeedMachListener;
        private readonly FcuSpeedManaged fcuSpeedManaged;
        private readonly FcuSpeedSelection fcuSpeedSelection;
        private readonly FcuTrackFpa fcuTrackFpa;
        private readonly FcuHeadingManaged fcuHeadingManaged;
        private readonly FcuHeadingSelected fcuHeadingSelected;
        private readonly FcuHeadingDashes fcuHeadingDashes;

        public FcuDisplayLeft(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            serial = sp.GetRequiredService<SerialPico>();
            (fcuSpeedMachListener = sp.GetRequiredService<FcuSpeedMachListener>()).PropertyChanged += Regenerate;
            (fcuSpeedManaged = sp.GetRequiredService<FcuSpeedManaged>()).PropertyChanged += Regenerate;
            (fcuSpeedSelection = sp.GetRequiredService<FcuSpeedSelection>()).PropertyChanged += Regenerate;
            (fcuTrackFpa = sp.GetRequiredService<FcuTrackFpa>()).PropertyChanged += Regenerate;
            (fcuHeadingManaged = sp.GetRequiredService<FcuHeadingManaged>()).PropertyChanged += Regenerate;
            (fcuHeadingSelected = sp.GetRequiredService<FcuHeadingSelected>()).PropertyChanged += Regenerate;
            (fcuHeadingDashes = sp.GetRequiredService<FcuHeadingDashes>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var speedMachLabel = fcuSpeedMachListener.IsMach ? " MACH" : "SPD  ";
            var hdgTrkLabel = fcuTrackFpa.IsHdgVS ? "HDG  " : "  TRK";
            var line1 = $"{speedMachLabel}  {hdgTrkLabel} LAT";

            var speedDot = fcuSpeedManaged.IsManaged ? '\x1' : ' ';
            var heading = fcuHeadingDashes.IsDashes || fcuHeadingSelected == -1 ? "---" : $"{(double)fcuHeadingSelected!:000}";
            var headingDot = fcuHeadingManaged.IsManaged ? '\x1' : ' ';
            var line2 = $"{Speed} {speedDot}  {heading}   {headingDot} ";

            serial.SendLine($"fcuTL={line1}");
            serial.SendLine($"fcuBL={line2}");
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
