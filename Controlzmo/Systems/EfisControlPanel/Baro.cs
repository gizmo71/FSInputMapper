using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using WASimCommander.CLI.Structs;
using static Controlzmo.GameControllers.AbstractRepeatingDoublePress;

namespace Controlzmo.Systems.EfisControlPanel
{
    [Component]
    [RequiredArgsConstructor]
    public partial class BaroKnob : ISettable<string>
    {
        private readonly JetBridgeSender sender;

        public string GetId() => "baroKnob";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            string command;
            if (value == "pull")
                command = simConnect.IsFenix ? "1 (>L:S_FCU_EFIS1_BARO_STD)" : @"(L:XMLVAR_Baro1_Mode) 2 | (>L:XMLVAR_Baro1_Mode)";
            else if (value == "push")
                command = simConnect.IsFenix ? "0 (>L:S_FCU_EFIS1_BARO_STD)" :
                    @"(L:XMLVAR_Baro1_Mode) 2 & 0 != if{ 2 } els{ 1 } (L:XMLVAR_Baro1_Mode) ^ (>L:XMLVAR_Baro1_Mode)";
            else if (simConnect.IsFenix && value!.EndsWith("Dec"))
                command = "(L:E_FCU_EFIS1_BARO) -- (>L:E_FCU_EFIS1_BARO)";
            else if (value == "hPaDec")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 == if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) near -- 16 * (>K:2:KOHLSMAN_SET) }";
            else if (value == "inHgDec")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 == if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) 0.3386389 / near -- 5.4182224 * (>K:2:KOHLSMAN_SET) } }";
            else if (simConnect.IsFenix && value!.EndsWith("Inc"))
                command = "(L:E_FCU_EFIS1_BARO) ++ (>L:E_FCU_EFIS1_BARO)";
            else if (value == "hPaInc")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 == if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) near ++ 16 * (>K:2:KOHLSMAN_SET) }";
            else if (value == "inHgInc")
                command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 == if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) 0.3386389 / near ++ 5.4182224 * (>K:2:KOHLSMAN_SET) } }";
            else if (value == "inHg")
                command = simConnect.IsFenix ? "0 (>L:S_FCU_EFIS1_BARO_MODE)" : "0 (>L:XMLVar_Baro_Selector_HPA_1)";
            else if (value == "hPa")
                command = simConnect.IsFenix ? "1 (>L:S_FCU_EFIS1_BARO_MODE)" : "1 (>L:XMLVar_Baro_Selector_HPA_1)";
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
        [SimVar("L:XMLVAR_Baro1_Mode", "", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 baro1Mode; // Bit 0: 1 QNH, 0 QFE; Bit 1: 2 standard, otherwise as bit 1
        [SimVar("L:I_FCU_EFIS1_QNH", "", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 baro1ModeFenix; // 0 Std, 1 QNH
        [SimVar("L:XMLVar_Baro_Selector_HPA_1", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 baro1Units; // InHg if 0, otherwise hPa
        [SimVar("L:S_FCU_EFIS1_BARO_MODE", "", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 baro1UnitsFenix; // 0 InHg, 1 hPa
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
            if (simConnect.IsFenix ? data.baro1ModeFenix == 1 : (data.baro1Mode & 2) == 0)
            {
                var value = (simConnect.IsFenix ? data.baro1UnitsFenix : data.baro1Units) == 0 ? data.kohlsmanHg * 100 : data.kohlsmanMB;
                composite = (!simConnect.IsFBW || (data.baro1Mode & 1) == 1 ? "N" : "F") + $"{value,4:0}";
            }
            serial.SendLine($"baro={composite}");
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class BaroPush : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        private readonly BaroKnob knob;
        private DateTime magicIfAfter = DateTime.MaxValue;
        public int GetButton() => UrsaMinorFighterR.BUTTON_MID_STICK_TRIM_FORE;

        public virtual void OnPress(ExtendedSimConnect sc)
        {
            knob.SetInSim(sc, "push");
            magicIfAfter = DateTime.UtcNow.AddMilliseconds(500);
        }

        public virtual void OnRelease(ExtendedSimConnect sc)
        {
            if (DateTime.UtcNow > magicIfAfter)
                sender.Execute(sc, "(>K:BAROMETRIC)");
            magicIfAfter = DateTime.MaxValue;
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class BaroPull : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly BaroKnob knob;
        public int GetButton() => UrsaMinorFighterR.BUTTON_MID_STICK_TRIM_AFT;
        public virtual void OnPress(ExtendedSimConnect sc) => knob.SetInSim(sc, "pull");
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class BaroUnits : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_MID_STICK_TRIM_PRESS;

        public virtual void OnPress(ExtendedSimConnect sc) {
            var lvar = sc.IsFenix ? "S_FCU_EFIS1_BARO_MODE" : "XMLVar_Baro_Selector_HPA_1";
            sender.Execute(sc, $"1 (L:{lvar}) - (>L:{lvar})");
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class RepeatingBaroChange
    {
        private readonly JetBridgeSender sender;
        private readonly SimConnectHolder holder;
        private Timer? timer;
        private int direction;

        internal void Start(int direction)
        {
            lock(this)
            {
                if (timer == null)
                    timer = new Timer(HandleTimer);
                this.direction = direction;
            }
            HandleTimer(null);
            timer.Change(200, 100);
        }

        internal void Stop() => timer!.Change(Timeout.Infinite, Timeout.Infinite);

        private Int32 adjustment = 0;

        private void HandleTimer(object? _) {
            var sc = holder.SimConnect;
            if (sc!.IsFBW)
            {
            }
            else if (sc!.IsFenix)
            {
                Interlocked.Add(ref adjustment, direction);
                sender.Execute(sc!, delegate() {
                    var toSend = Interlocked.Exchange(ref adjustment, 0);
                    return toSend == 0 ? null : $"(L:E_FCU_EFIS1_BARO) {toSend} + (>L:E_FCU_EFIS1_BARO)";
                });
            }
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class BaroInc : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly RepeatingBaroChange change;
        public int GetButton() => UrsaMinorFighterR.BUTTON_MID_STICK_TRIM_RIGHT;
        public virtual void OnPress(ExtendedSimConnect sc) => change.Start(+1);
        public virtual void OnRelease(ExtendedSimConnect sc) => change.Stop();
    }

    [Component, RequiredArgsConstructor]
    public partial class BaroDec : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly RepeatingBaroChange change;
        public int GetButton() => UrsaMinorFighterR.BUTTON_MID_STICK_TRIM_LEFT;
        public virtual void OnPress(ExtendedSimConnect sc) => change.Start(-1);
        public virtual void OnRelease(ExtendedSimConnect sc) => change.Stop();
    }
}
