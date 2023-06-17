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
        private readonly FcuSpeed fcuSpeedMachListener;
        private readonly FcuTrackFpa fcuTrackFpa;

        public FcuDisplayTopLeft(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
            (fcuSpeedMachListener = sp.GetRequiredService<FcuSpeed>()).PropertyChanged += Regenerate;
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
        private readonly FcuHeadingManaged fcuHeadingManaged;
        private readonly FcuSpeed fcuSpeed;
        private readonly FcuHeadingSelected fcuHeadingSelected;
        private readonly FcuHeadingDashes fcuHeadingDashes;

        public FcuDisplayBottomLeft(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
            (fcuHeadingManaged = sp.GetRequiredService<FcuHeadingManaged>()).PropertyChanged += Regenerate;
            (fcuSpeed = sp.GetRequiredService<FcuSpeed>()).PropertyChanged += Regenerate;
            (fcuHeadingSelected = sp.GetRequiredService<FcuHeadingSelected>()).PropertyChanged += Regenerate;
            (fcuHeadingDashes = sp.GetRequiredService<FcuHeadingDashes>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var speedDot = fcuSpeed.IsManaged ? '\x1' : ' ';
            var heading = fcuHeadingDashes.IsDashes || fcuHeadingSelected == -1 ? "---" : $"{(double)fcuHeadingSelected!:000}";
            var headingDot = fcuHeadingManaged.IsManaged ? '\x1' : ' ';
            var line2 = $"{Speed} {speedDot}  {heading}   {headingDot} ";
            serial.SendLine($"fcuBL={line2}");
        }

        private string Speed
        {
            get
            {
                //TODO: move this into the supplying object.
                string speed;
                var selection = (double)fcuSpeed.DisplayedSpeed;
                if (selection >= 0.10 && selection < 1.0)
                    speed = $"{selection:0.00}";
                else if (selection >= 100 && selection < 1000) // Max 399 in the A32NX
                    speed = $" {selection:000}";
                else
                    speed = " ---";
                return speed;
            }
        }
    }
}
