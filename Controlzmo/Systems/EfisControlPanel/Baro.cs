using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.EfisControlPanel
{
    [Component]
    public class BaroKnob : ISettable<string>
    {
        private readonly JetBridgeSender sender;

        public BaroKnob(JetBridgeSender sender) => this.sender = sender;

        public string GetId() => "baroKnob";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            string command;
            if (value == "pull")
                command = @"(L:XMLVAR_Baro1_Mode) 2 | (>L:XMLVAR_Baro1_Mode)";
            else if (value == "push")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 != if{ 2 } els{ 1 } (L:XMLVAR_Baro1_Mode) ^ (>L:XMLVAR_Baro1_Mode)";
            else if (value == "hPaDec")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 == if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) near -- 16 * (>K:2:KOHLSMAN_SET) }";
            else if (value == "inHgDec")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 == if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) 0.3386389 / near -- 5.4182224 * (>K:2:KOHLSMAN_SET) } }";
            else if (value == "hPaInc")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 == if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) near ++ 16 * (>K:2:KOHLSMAN_SET) }";
            else if (value == "inHgInc")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 == if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) 0.3386389 / near ++ 5.4182224 * (>K:2:KOHLSMAN_SET) } }";
            else if (value == "inHg")
                command = "0 (>L:XMLVar_Baro_Selector_HPA_1)";
            else if (value == "hPa")
                command = "1 (>L:XMLVar_Baro_Selector_HPA_1)";
            else
                return;

            command = command.Trim();

            sender.Execute(simConnect, command!);
        }
    }

    [Component]
    public class Baro1Mode : LVar, IOnSimStarted
    {
        public Baro1Mode(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "XMLVAR_Baro1_Mode";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
        public bool isQnh { get => ((int?)Value & 1) == 1; } // Otherwise QFE
        public bool isStd { get => ((int?)Value & 2) == 2; } // Otherwise QFE or QNH
    }

    [Component]
    public class Baro1Units : LVar, IOnSimStarted
    {
        public Baro1Units(IServiceProvider serviceProvider) : base(serviceProvider) { }
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
        protected override string LVarName() => "XMLVar_Baro_Selector_HPA_1";
        public bool isInHg { get => Value == 0; } // Otherwise hPa
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BaroData
    {
        [SimVar("KOHLSMAN SETTING MB", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB;
        [SimVar("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT32, 0.01f)]
        public float kohlsmanHg;
    }

    [Component]
    public class BaroDisplay : DataListener<BaroData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;
        private readonly Baro1Mode baro1Mode;
        private readonly Baro1Units baro1Units;
        private BaroData currentSetting = new BaroData { kohlsmanHg = 0, kohlsmanMB = 0 };

        public BaroDisplay(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
            baro1Mode = sp.GetRequiredService<Baro1Mode>();
            baro1Units = sp.GetRequiredService<Baro1Units>();

            baro1Mode.PropertyChanged += Regenerate;
            baro1Units.PropertyChanged += Regenerate;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, BaroData data)
        {
            if (!baro1Mode.isStd || currentSetting.kohlsmanMB == 0)
                currentSetting = data;
            Regenerate(this, null);
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args)
        {
            string composite = "SStd ";
            if (!baro1Mode.isStd)
            {
                var value = baro1Units.isInHg ? currentSetting.kohlsmanHg * 100 : currentSetting.kohlsmanMB;
                composite = (baro1Mode.isQnh ? "N" : "F") + $"{value,4:0}";
            }
            serial.SendLine($"baro={composite}");
        }
    }
}
