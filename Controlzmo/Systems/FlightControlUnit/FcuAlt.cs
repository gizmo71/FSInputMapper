using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Threading;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component, RequiredArgsConstructor]
    public partial class FcuAltPulled : IEvent, IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_NEAR_DOWN;
        public void OnPress(ExtendedSimConnect sc) => SetInSim(sc, true);
        public string SimEvent() => "A32NX.FCU_ALT_PULL";

        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_ALTITUDE) ++ (>L:S_FCU_ALTITUDE)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_FCU_ALTITUDE_PULL_COMMAND)");
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, "1 (>L:MSATR_FGCP_ALT)");
            else if (simConnect.IsB78x)
                sender.Execute(simConnect, "(>B:AUTOPILOT_ALTITUDE_SYNC_PUSH)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class FcuAltPushed : IEvent, IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_NEAR_UP;
        public void OnPress(ExtendedSimConnect sc) => SetInSim(sc, true);

        public string SimEvent() => "A32NX.FCU_ALT_PUSH";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_ALTITUDE) -- (>L:S_FCU_ALTITUDE)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_FCU_ALTITUDE_PUSH_COMMAND)");
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, "1 (>L:MSATR_FGCP_VNAV)");
            else if (simConnect.IsB78x)
                sender.Execute(simConnect, "(>B:AUTOPILOT_VNAV_MODE_ON)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    public class FcuAltInc : IEvent { public string SimEvent() => "A32NX.FCU_ALT_INC"; }

    [Component]
    public class FcultDec : IEvent { public string SimEvent() => "A32NX.FCU_ALT_DEC"; }

    [Component, RequiredArgsConstructor]
    public partial class FcuAltDelta
    {
        private readonly FcuAltInc inc;
        private readonly FcultDec dec;
        private readonly JetBridgeSender sender;
        private readonly InputEvents inputEvents;

        private Int32 lvarAdjustment = 0;

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix || simConnect.IsAtr || simConnect.IsB78x) {
                Interlocked.Add(ref lvarAdjustment, value);
                sender.Execute(simConnect, ExecuteLvar);
            }
            else
                while (value != 0)
                {
                    if (simConnect.IsIni330)
                        inputEvents.Send(simConnect, "AIRLINER_MCU_ALT", (double) Math.Sign(value));
                    else if (simConnect.IsIniBuilds)
                        inputEvents.Send(simConnect, "INSTRUMENT_FCU_ALT_KNOB", (double) Math.Sign(value));
                    else
                        simConnect.SendEvent(value < 0 ? dec : inc);
                    value -= (short)Math.Sign(value);
                }
        }

        private String? ExecuteLvar(ExtendedSimConnect simConnect)
        {
            var toSend = Interlocked.Exchange(ref lvarAdjustment, 0);
            if (toSend == 0) return null;
            if (simConnect.IsB78x)
                return $"3 (A:AUTOPILOT ALTITUDE LOCK VAR:3, feet) (L:XMLVAR_Autopilot_Altitude_Increment) {toSend} * + (>K:2:AP_ALT_VAR_SET_ENGLISH)";
            var lvar = simConnect.IsAtr ? "MSATR_FGCP_ALTSEL_DELTA" : "E_FCU_ALTITUDE";
            return $"(L:{lvar}) {toSend} + (>L:{lvar})";
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class FcuAltIncrement
    {
        private readonly JetBridgeSender sender;

//TODO: can we use one of these LVars to track our increment for the ATR?
        public void SetInSim(ExtendedSimConnect simConnect, uint value) {
            string command;
            if (simConnect.IsB78x)
                command = "(>B:AUTOPILOT_Altitude_Increment_Toggle)";
            else if (simConnect.IsFenix)
                command = toggleOrSet("S_FCU_ALTITUDE_SCALE", value);
            else if (simConnect.IsIniBuilds)
            {
                command = "1 (>L:INI_FCU_ALTITUDE_MODE_COMMAND)";
                if (value != 0)
                {
                    var current = simConnect.IsIni330 ? "INI_ALTITUDE_STATE" : "__FCU_ALT_UNITSISPRESSED";
                    command = $$"""{{value / 1000}}.0 (L:{{current}}) != if{ {{command}} }""";
                }
            }
            else if (value == 0)
                command = $"(>K:A32NX.FCU_ALT_INCREMENT_TOGGLE)";
            else
                command = $"{value} (>K:A32NX.FCU_ALT_INCREMENT_SET)";
            sender.Execute(simConnect, command);
        }

        private string toggleOrSet(string lvar, uint value)
        {
            if (value == 0) // Toggle
                    return $"1 (L:{lvar}) - (>L:{lvar})";
            else // Set
                    return (value == 1000 ? 1 : 0) + $" (>L:{lvar})";
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class FcuAltRepeatingDoublePress : AbstractRepeatingDoublePress
    {
        private readonly FcuAltDelta delta;
        private readonly FcuAltIncrement bothPressed;

        protected override void UpAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, +1);
        protected override void DownAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, -1);
        protected override void BothAction(ExtendedSimConnect? simConnect) => bothPressed.SetInSim(simConnect!, 0);
    }

    [Component, RequiredArgsConstructor]
    public partial class IncOrToggleFcuAlt : RepeatingDoublePressButton<UrsaMinorFighterR, FcuAltRepeatingDoublePress>
    {
        [Property]
        private readonly FcuAltRepeatingDoublePress _controller;
        int IButtonCallback<UrsaMinorFighterR>.GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_FAR_LEFT_UP;
        AbstractRepeatingDoublePress.Direction RepeatingDoublePressButton<UrsaMinorFighterR, FcuAltRepeatingDoublePress>.GetDirection()
            => AbstractRepeatingDoublePress.Direction.Up;
    }

    [Component, RequiredArgsConstructor]
    public partial class DecOrToggleFcuAlt : RepeatingDoublePressButton<UrsaMinorFighterR, FcuAltRepeatingDoublePress>
    {
        [Property]
        private readonly FcuAltRepeatingDoublePress _controller;
        int IButtonCallback<UrsaMinorFighterR>.GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_FAR_LEFT_DOWN;
        AbstractRepeatingDoublePress.Direction RepeatingDoublePressButton<UrsaMinorFighterR, FcuAltRepeatingDoublePress>.GetDirection()
            => AbstractRepeatingDoublePress.Direction.Down;
    }
}
