using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

//TODO: A380 has pre-set of baro during STD.
namespace Controlzmo.Systems.EfisControlPanel
{
    [Component, RequiredArgsConstructor]
    public partial class BaroKnob : ISettable<string>
    {
        private readonly JetBridgeSender sender;
        private readonly BaroPull baroPull;
        private readonly BaroPush baroPush;
        // We can't inject RepeatingBaroChange because we're something the serial code needs, so we end up in a deadly embrace.
        private readonly IServiceProvider sp;

        public string GetId() => "baroKnob";

        private RepeatingBaroChange baroChange {  get => sp.GetRequiredService<RepeatingBaroChange>(); }

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
/* From WinWing EFIS code for left side
1 (>K:KOHLSMAN_INC)
0 (>L:XMLVAR_Baro1_Mode)
(L:A310_INSTRUMENTS_LINKED, Bool) if{
   (A:KOHLSMAN SETTING MB:1, millibar) 16 * (>K:2:KOHLSMAN_SET)
   (A:KOHLSMAN SETTING MB:1, millibar) 16 * (>K:3:KOHLSMAN_SET)
   0 (>L:XMLVAR_Baro2_Mode)
   0 (>L:XMLVAR_Baro3_Mode)
}                                       */
            string command;
            if (value == "pull")
            {
                baroPull.OnPress(simConnect);
                return;
            }
            else if (value == "push")
            {
                baroPush.PressAndRelease(simConnect);
                return;
            }
            else if (value!.EndsWith("Dec"))
            {
                baroChange.Click(-1);
                return;
            }
            else if (value!.EndsWith("Inc"))
            {
                baroChange.Click(1);
                return;
            }
            else if (value == "inHg")
                command = simConnect.IsFenix ? "0 (>L:S_FCU_EFIS1_BARO_MODE)" : "0 (>L:XMLVar_Baro_Selector_HPA_1)";
            else if (value == "hPa")
                command = simConnect.IsFenix ? "1 (>L:S_FCU_EFIS1_BARO_MODE)" : "1 (>L:XMLVar_Baro_Selector_HPA_1)";
            else
                return;

            command = command.Trim();
System.Console.WriteLine($"-> {value} led to {command}");

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

    [Component, RequiredArgsConstructor]
    public partial class BaroDisplay : DataListener<BaroData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;
        [Property]
        private Boolean _isStd;
        [Property]
        private Boolean _isInHg;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, BaroData data)
        {
            if (simConnect.IsFenix)
            {
                data.baro1Mode = data.baro1ModeFenix == 1 ? 1 : 3;
                data.baro1Units = data.baro1UnitsFenix;
            }
            else if (simConnect.IsIniBuilds)
                data.baro1Mode |= 1;

            string composite = "SStd ";
            _isInHg = data.baro1Units == 0;
            if ((data.baro1Mode & 2) == 0)
            {
                var value = _isInHg ? data.kohlsmanHg * 100 : data.kohlsmanMB;
                composite = ((data.baro1Mode & 1) == 1 ? "N" : "F") + $"{value,4:0}";
                _isStd = false;
            }
            else
                _isStd = true;

            serial.SendLine($"baro={composite}");
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class BaroPush : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        private readonly SetSeaLevelPressure setMagic;
        private DateTime magicIfAfter = DateTime.MaxValue;
        public int GetButton() => UrsaMinorFighterR.BUTTON_MID_STICK_TRIM_FORE;

        public virtual void OnPress(ExtendedSimConnect sc)
        {
            magicIfAfter = DateTime.UtcNow.AddMilliseconds(500);
        }

        public virtual void OnRelease(ExtendedSimConnect sc)
        {
            if (DateTime.UtcNow > magicIfAfter)
                setMagic.SetLocal(sc);
            else
            {
                var command = @"(L:XMLVAR_Baro1_Mode) 2 & 0 != if{ 2 } els{ 1 } (L:XMLVAR_Baro1_Mode) ^ (>L:XMLVAR_Baro1_Mode)";
                if (sc.IsFenix) command = "(L:S_FCU_EFIS1_BARO_STD) -- (>L:S_FCU_EFIS1_BARO_STD)";
                else if (sc.IsIniBuilds) command = @"1 (>L:INI_1_ALTIMETER_PUSH_COMMAND)";
                sender.Execute(sc, command);
            }
            magicIfAfter = DateTime.MaxValue;
        }

        internal void PressAndRelease(ExtendedSimConnect sc)
        {
            OnPress(sc);
            OnRelease(sc);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SeaLevelData
    {
        [SimVar("SEA LEVEL PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float seaLevelPressure;
    }

    [Component]
    public class BarometricEvent : IEvent { public string SimEvent() => "BAROMETRIC"; }

    [Component, RequiredArgsConstructor]
    public partial class SetSeaLevelPressure : DataListener<SeaLevelData>
    {
        private readonly JetBridgeSender sender;
        private readonly BarometricEvent _event;

        internal void SetLocal(ExtendedSimConnect simConnect)
        {
#if false
            if (simConnect.IsIniBuilds)
                simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
            else
#endif
                simConnect.SendEvent(_event);
        }

        public override void Process(ExtendedSimConnect simConnect, SeaLevelData data)
        {
            sender.Execute(simConnect, $"{data.seaLevelPressure * 16} (>K:2:KOHLSMAN_SET)");
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class BaroPull : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        public int GetButton() => UrsaMinorFighterR.BUTTON_MID_STICK_TRIM_AFT;
        public virtual void OnPress(ExtendedSimConnect simConnect)
        {
            var command = @"(L:XMLVAR_Baro1_Mode) 2 | (>L:XMLVAR_Baro1_Mode)";
            if (simConnect.IsFenix)
                command = @"(L:S_FCU_EFIS1_BARO_STD) ++ (>L:S_FCU_EFIS1_BARO_STD)";
            else if (simConnect.IsIniBuilds)
                command = @"1 (>L:INI_1_ALTIMETER_PULL_COMMAND)";
            sender.Execute(simConnect, command);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class BaroUnits : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;
        private readonly InputEvents inputEvents;

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
        private readonly BaroDisplay display;
        private readonly InputEvents inputEvents;
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
            if (sc!.IsIni330)
            {
                inputEvents.Send(sc!, "AIRLINER_CPT_BARO_SET", direction * 1.0);
                return;
            }
            else if (sc!.IsIni320 || sc!.IsIni321)
            {
                inputEvents.Send(sc!, "INSTRUMENT_QNH_CPT_KNOB", direction * 1.0);
                return;
            }

            Interlocked.Add(ref adjustment, direction);
            if (sc!.IsFBW)
            {
                sender.Execute(sc!, delegate() {
                    var toSend = Interlocked.Exchange(ref adjustment, 0);
                    // Sadly to do all this in RPN just too long - we're restricted to 127 chars. :-( So we have to track state instead.
                    if (toSend == 0 || display.IsStd) return null;
                    var divideFactor = display.IsInHg ? 0.3386389 : 1.0;
                    var multiplyFactor = display.IsInHg ? 5.4182224 : 16.0;
                    return $"1 (A:KOHLSMAN SETTING MB:1, mbars) {divideFactor} / near {toSend} + {multiplyFactor} * (>K:2:KOHLSMAN_SET)";
                });
            }
            else if (sc!.IsFenix)
            {
                sender.Execute(sc!, delegate() {
                    var toSend = Interlocked.Exchange(ref adjustment, 0);
                    return toSend == 0 ? null : $"(L:E_FCU_EFIS1_BARO) {toSend} + (>L:E_FCU_EFIS1_BARO)";
                });
            }
        }

        internal void Click(int v)
        {
            Start(v);
            Stop();
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
