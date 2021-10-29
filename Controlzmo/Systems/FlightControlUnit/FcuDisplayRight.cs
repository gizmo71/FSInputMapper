using Controlzmo.Hubs;
using Controlzmo.Serial;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuDisplayRight : CreateOnStartup
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly SerialPico serial;
        private readonly FcuAltManaged fcuAltManaged;
        private readonly FcuAltListener fcuAltListener;
        private readonly FcuTrackFpa fcuTrackFpa;
        private readonly FcuVsState fcuVsState;
        private readonly FcuVsSelected fcuVsSelected;
        private readonly FcuFpaSelected fcuFpaSelected;

        public FcuDisplayRight(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            serial = sp.GetRequiredService<SerialPico>();
            (fcuAltManaged = sp.GetRequiredService<FcuAltManaged>()).PropertyChanged += Regenerate;
            (fcuAltListener = sp.GetRequiredService<FcuAltListener>()).PropertyChanged += Regenerate;
            (fcuTrackFpa = sp.GetRequiredService<FcuTrackFpa>()).PropertyChanged += Regenerate;
            (fcuVsState = sp.GetRequiredService<FcuVsState>()).PropertyChanged += Regenerate;
            (fcuVsSelected = sp.GetRequiredService<FcuVsSelected>()).PropertyChanged += Regenerate;
            (fcuFpaSelected = sp.GetRequiredService<FcuFpaSelected>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            var managed = fcuAltManaged.IsManaged ? "*" : " ";
            var line1 = "ALT -LVL/CH- " + (fcuTrackFpa.IsHdgVS ? "V/S" : "FPA");
            var line2 = $"{fcuAltListener.Current.fcuAlt:00000}   {managed}  {VS}";
            serial.SendLine($"fcuTR={line1}");
            serial.SendLine($"fcuBR={line2}");
            hub.Clients.All.SetFromSim("fcuDisplayRight", $"{line1}\n{line2}");
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
