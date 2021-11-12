using Controlzmo.Serial;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuDisplayTopLeft : CreateOnStartup
    {
        private readonly SerialPico serial;
        private readonly FcuSpeedMachListener fcuSpeedMachListener;
        private readonly FcuTrackFpa fcuTrackFpa;

        public FcuDisplayTopLeft(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
            (fcuSpeedMachListener = sp.GetRequiredService<FcuSpeedMachListener>()).PropertyChanged += Regenerate;
            (fcuTrackFpa = sp.GetRequiredService<FcuTrackFpa>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var speedMachLabel = fcuSpeedMachListener.IsMach ? " MACH" : "SPD  ";
            var hdgTrkLabel = fcuTrackFpa.IsHdgVS ? "HDG  " : "  TRK";
            var line1 = $"{speedMachLabel}  {hdgTrkLabel} LAT";
            serial.SendLine($"fcuTL={line1}");
        }
    }

    [Component]
    public class FcuDisplayBottomLeft : CreateOnStartup
    {
        private readonly SerialPico serial;
        private readonly FcuSpeedManaged fcuSpeedManaged;
        private readonly FcuHeadingManaged fcuHeadingManaged;
        private readonly FcuSpeedSelection fcuSpeedSelection;
        private readonly FcuHeadingSelected fcuHeadingSelected;
        private readonly FcuHeadingDashes fcuHeadingDashes;

        public FcuDisplayBottomLeft(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
            (fcuSpeedManaged = sp.GetRequiredService<FcuSpeedManaged>()).PropertyChanged += Regenerate;
            (fcuHeadingManaged = sp.GetRequiredService<FcuHeadingManaged>()).PropertyChanged += Regenerate;
            (fcuSpeedSelection = sp.GetRequiredService<FcuSpeedSelection>()).PropertyChanged += Regenerate;
            (fcuHeadingSelected = sp.GetRequiredService<FcuHeadingSelected>()).PropertyChanged += Regenerate;
            (fcuHeadingDashes = sp.GetRequiredService<FcuHeadingDashes>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var speedDot = fcuSpeedManaged.IsManaged ? '\x1' : ' ';
            var heading = fcuHeadingDashes.IsDashes || fcuHeadingSelected == -1 ? "---" : $"{(double)fcuHeadingSelected!:000}";
            var headingDot = fcuHeadingManaged.IsManaged ? '\x1' : ' ';
            var line2 = $"{Speed} {speedDot}  {heading}   {headingDot} ";
            serial.SendLine($"fcuBL={line2}");
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
