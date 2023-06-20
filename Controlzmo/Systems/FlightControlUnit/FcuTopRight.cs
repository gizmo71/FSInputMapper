using Controlzmo.Serial;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.FlightControlUnit
{
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
