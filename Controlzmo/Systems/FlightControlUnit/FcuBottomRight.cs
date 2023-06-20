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
}
