using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.Systems.JetBridge;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BaroData
    {
        [SimVar("KOHLSMAN SETTING MB", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB;
        [SimVar("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT32, 0.01f)]
        public float kohlsmanHg;
        [SimVar("L:XMLVar_Baro_Selector_HPA_1", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 baro1Units; // InHg if 0, otherwise hPa
        [SimVar("L:XMLVAR_Baro1_Mode", "", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 baro1Mode; // Bit 0: 1 QNH, 0 QFE; Bit 1: 2 standard, otherwise as bit 1
    }

    [Component]
    public class BaroDisplay : DataListener<BaroData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;

        public BaroDisplay(IServiceProvider sp) => serial = sp.GetRequiredService<SerialPico>();

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, BaroData data)
        {
            string composite = "SStd ";
            if ((data.baro1Mode & 2) == 0)
            {
                var value = data.baro1Units == 0 ? data.kohlsmanHg * 100 : data.kohlsmanMB;
                composite = ((data.baro1Mode & 1) == 1 ? "N" : "F") + $"{value,4:0}";
            }
            serial.SendLine($"baro={composite}");
        }
    }
}
