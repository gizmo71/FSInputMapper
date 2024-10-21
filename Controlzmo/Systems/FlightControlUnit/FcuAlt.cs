using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Threading;
using WASimCommander.CLI.Structs;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltPulled : ISettable<bool>, IEvent, IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_NEAR_DOWN;
        public void OnPress(ExtendedSimConnect sc) => SetInSim(sc, true);

        public string SimEvent() => "A32NX.FCU_ALT_PULL";
        public string GetId() => "fcuAltPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_ALTITUDE) ++ (>L:S_FCU_ALTITUDE)");
            else if (simConnect.IsIni320)
                sender.Execute(simConnect, "1 (>L:INI_FCU_ALTITUDE_PULL_COMMAND)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltPushed : ISettable<bool>, IEvent, IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_NEAR_UP;
        public void OnPress(ExtendedSimConnect sc) => SetInSim(sc, true);

        public string SimEvent() => "A32NX.FCU_ALT_PUSH";
        public string GetId() => "fcuAltPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_FCU_ALTITUDE) -- (>L:S_FCU_ALTITUDE)");
            else if (simConnect.IsIni320)
                sender.Execute(simConnect, "1 (>L:INI_FCU_ALTITUDE_PUSH_COMMAND)");
            else
                simConnect.SendEvent(this);
        }
    }

    [Component]
    public class FcuAltInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_INC";
    }

    [Component]
    public class FcultDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_DEC";
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltDelta : ISettable<Int16>
    {
        private readonly FcuAltInc inc;
        private readonly FcultDec dec;
        private readonly JetBridgeSender sender;

        private Int32 fenixAdjustment = 0;

        public string GetId() => "fcuAltDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            if (simConnect.IsFenix) {
                Interlocked.Add(ref fenixAdjustment, value);
                sender.Execute(simConnect, delegate() {
                    var toSend = Interlocked.Exchange(ref fenixAdjustment, 0);
                    var op = toSend < 0 ? "-" : "+";
                    return toSend == 0 ? null : $"(L:E_FCU_ALTITUDE) {Math.Abs(toSend)} {op} (>L:E_FCU_ALTITUDE)";
                });
            }
            else
                while (value != 0)
                {
                    if (simConnect.IsIni320)
                    {
                        var dir = value < 0 ? "DN" : "UP";
                        sender.Execute(simConnect, $"1 (>L:INI_ALTITUDE_DIAL_{dir}_COMMAND)");
                    }
                    else
                        simConnect.SendEvent(value < 0 ? dec : inc);
                    value -= (short)Math.Sign(value);
                }
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltIncrement : ISettable<uint>
    {
        private readonly JetBridgeSender sender;
        public string GetId() => "fcuAltIncrement";
        public void SetInSim(ExtendedSimConnect simConnect, uint value) {
            string command;
            if (simConnect.IsFenix)
                command = toggleOrSet("S_FCU_ALTITUDE_SCALE", value);
            else if (simConnect.IsIni320)
                command = toggleOrSet("INI_FCU_ALTITUDE_MODE_COMMAND", value);
            else if (value == 0)
                command = $"(>K:A32NX.FCU_ALT_INCREMENT_TOGGLE)";
            else
                command = $"{value} (>K:A32NX.FCU_ALT_INCREMENT_SET)";
            sender.Execute(simConnect, command);
        }

        private string toggleOrSet(string lvar, uint value)
        {
            if (value == 0)
                    return $"1 (L:{lvar}) - (>L:{lvar})";
            else
                    return (value == 1000 ? 1 : 0) + $" (>L:{lvar})";
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuAltRepeatingDoublePress : AbstractRepeatingDoublePress
    {
        private readonly FcuAltDelta delta;
        private readonly FcuAltIncrement notAToggle;

        protected override void UpAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, +1);
        protected override void DownAction(ExtendedSimConnect? simConnect) => delta.SetInSim(simConnect!, -1);
        protected override void BothAction(ExtendedSimConnect? simConnect) => notAToggle.SetInSim(simConnect!, 0);
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class IncOrToggleFcuAlt : RepeatingDoublePressButton<UrsaMinorFighterR>
    {
        protected override AbstractRepeatingDoublePress Controller { get => _controller; } //[Property]
        private readonly FcuAltRepeatingDoublePress _controller;
        public override int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_FAR_LEFT_UP;
        protected override AbstractRepeatingDoublePress.Direction GetDirection() => AbstractRepeatingDoublePress.Direction.Up;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class DecOrToggleFcuAlt : RepeatingDoublePressButton<UrsaMinorFighterR>
    {
        protected override AbstractRepeatingDoublePress Controller { get => _controller; } //[Property]
        private readonly FcuAltRepeatingDoublePress _controller;
        public override int GetButton() => UrsaMinorFighterR.BUTTON_RIGHT_BASE_FAR_LEFT_DOWN;
        protected override AbstractRepeatingDoublePress.Direction GetDirection() => AbstractRepeatingDoublePress.Direction.Down;
    }
}
