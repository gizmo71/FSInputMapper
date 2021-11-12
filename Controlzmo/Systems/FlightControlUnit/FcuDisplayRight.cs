using Controlzmo.Serial;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuDisplayTopRight : CreateOnStartup
    {
        private readonly SerialPico serial;
        private readonly FcuTrackFpa fcuTrackFpa;

        public FcuDisplayTopRight(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
            (fcuTrackFpa = sp.GetRequiredService<FcuTrackFpa>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var line1 = "ALT \x4LVL/CH\x5 " + (fcuTrackFpa.IsHdgVS ? "V/S" : "FPA");
            serial.SendLine($"fcuTR={line1}");
        }
    }

    [Component]
    public class FcuDisplayBottomRight : CreateOnStartup
    {
        private readonly SerialPico serial;
        private readonly FcuAltListener fcuAltListener;
        private readonly FcuVsState fcuVsState;
        private readonly FcuVsSelected fcuVsSelected;
        private readonly FcuFpaSelected fcuFpaSelected;
        private readonly FcuTrackFpa fcuTrackFpa;
        private readonly FcuAltManaged fcuAltManaged;

        public FcuDisplayBottomRight(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
            (fcuAltListener = sp.GetRequiredService<FcuAltListener>()).PropertyChanged += Regenerate;
            (fcuVsState = sp.GetRequiredService<FcuVsState>()).PropertyChanged += Regenerate;
            (fcuVsSelected = sp.GetRequiredService<FcuVsSelected>()).PropertyChanged += Regenerate;
            (fcuFpaSelected = sp.GetRequiredService<FcuFpaSelected>()).PropertyChanged += Regenerate;
            (fcuTrackFpa = sp.GetRequiredService<FcuTrackFpa>()).PropertyChanged += Regenerate;
            (fcuAltManaged = sp.GetRequiredService<FcuAltManaged>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var managed = fcuAltManaged.IsManaged ? '\x1' : ' ';
            var line2 = $"{fcuAltListener.Current.fcuAlt:00000}   {managed}  {VS}";
            serial.SendLine($"fcuBR={line2}");
        }

        private string VS
        {
            get
            {
                string vs;
                if (fcuVsState.IsIdle)
                    vs = "-----";
                else if (fcuTrackFpa.IsTrkFpa)
                    vs = $"{(double)fcuFpaSelected!:+#0.0;-#0.0} ";
                else
                    vs = $"{fcuVsSelected / 100:+00;-00}oo";
                return vs;
            }
        }
    }
}
