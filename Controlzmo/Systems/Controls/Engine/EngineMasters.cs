using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Controls.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineControlSelectData
    {
        [SimVar("ENGINE CONTROL SELECT", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 bitFlags; // LSB engine 1 etc
        [SimVar("FUELSYSTEM VALVE SWITCH:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fsvs1;
        [SimVar("FUELSYSTEM VALVE SWITCH:2", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fsvs2;
        [SimVar("FUELSYSTEM VALVE SWITCH:3", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fsvs3;
        [SimVar("FUELSYSTEM VALVE SWITCH:4", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fsvs4;
    };

    [Component, RequiredArgsConstructor]
    public partial class EngineControlSelectListener : DataListener<EngineControlSelectData>, IButtonCallback<UrsaMinorFighterR>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public int GetButton() => UrsaMinorFighterR.BUTTON_FAR_TRIGGER_PUSH;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public virtual void OnPress(ExtendedSimConnect sc) => sc.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
        public override void Process(ExtendedSimConnect simConnect, EngineControlSelectData data)
        {
            // By default MSFS sets the correct value based on number of engines.
            //hub.Clients.All.Speak($"bits {data.bitFlags}");
            //hub.Clients.All.Speak($"Fuel system valves {data.fsvs1} {data.fsvs2} {data.fsvs3} {data.fsvs4}");
        }
    }

    // UI: SET ENGINE n FUEL VALVE
    [Component] public class FuelSystemValveSetEvent : IEvent { public string SimEvent() => "FUELSYSTEM_VALVE_SET"; }
    //[Component] public class FuelSystemValveCloseEvent : IEvent { public string SimEvent() => "FUELSYSTEM_VALVE_CLOSE"; }
    //[Component] public class FuelSystemValveOpenEvent : IEvent { public string SimEvent() => "FUELSYSTEM_VALVE_OPEN"; }
    [Component] public class FuelSystemPumpOnEvent : IEvent { public string SimEvent() => "FUELSYSTEM_PUMP_ON"; }
    [Component] public class FuelSystemPumpOffEvent : IEvent { public string SimEvent() => "FUELSYSTEM_PUMP_OFF"; }
//TODO: look at ENGINE CONTROL SELECT instead of per-engine events...
//[Component] public class EngineMaster1SetEvent : IEvent { public string SimEvent() => "ENGINE_MASTER_1_SET"; }
    // UI: SET ENGINE MASTER n
    [Component] public class EngineMasterSetEvent : IEvent { public string SimEvent() => "ENGINE_MASTER_SET"; }
[Component] public class Starter1SetEvent : IEvent { public string SimEvent() => "STARTER1_SET"; }
[Component] public class Starter2SetEvent : IEvent { public string SimEvent() => "STARTER2_SET"; }
// Get errors back from the generic one - may not be needed with FBW...
    // UI: SET STARTER n
    [Component] public class StarterSetEvent : IEvent { public string SimEvent() => "STARTER_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class EnginerMasterAction
    {
        private readonly FuelSystemValveSetEvent fuelSystemValveSet;
        private readonly FuelSystemPumpOnEvent fuelSystemPumpOn;
        private readonly FuelSystemPumpOffEvent fuelSystemPumpOff;
        private readonly StarterSetEvent starterSetEvent;
        private readonly EngineMasterSetEvent engineMasterSetEvent;
        private readonly JetBridgeSender sender;

        internal void perform(ExtendedSimConnect sc, Boolean isLeft, Boolean isOn)
        {
            var value = isOn ? 1u : 0u;

            if (sc.IsFenix)
            {
                var engineId = isLeft ? 1 : 2;
                sender.Execute(sc, $"{value} (>L:S_ENG_MASTER_{engineId})");
                return;
            }

/*TODO: the A400M has three positions for each switch, "off", "feather" (used during startup), and "run" (one AVAIL has been shown).
  We should go to "feather" initially (as we do, in fact), but then automatically switch to "run" once it's "up". */
            var is4engined = sc.IsA380X || sc.IsB748 || sc.IsIni400M;
            var first = isLeft ? 1u : (is4engined ? 3u : 2u);
            var last = first + (is4engined ? 1u : 0u);
/*TODO: seems to have stopped working for the A380X, at least in MSFS2024 :-(
Toggling on only toggles 1/2, as if we're not 4-engined.
Toggling off turns both off but only sort of... :-o */
            /*var newData = new EngineControlSelectData() { bitFlags = is4engined ? (isLeft ? 3 : 12) : (isLeft ? 1 : 2) };
Console.Error.WriteLine($"newData flags {newData.bitFlags} value {value} isLeft {isLeft}");
            sc.SendDataOnSimObject(newData);*/
            for (var engine = first; engine <= last; ++engine) {
                sc.SendEventEx1(fuelSystemValveSet, engine, value);
                //sc.SendEvent(isOn ? fuelSystemPumpOn : fuelSystemPumpOff, engine);
                //sc.SendEvent(engineMasterSetEvent, value);
                //sc.SendEvent(starterSetEvent, value);
            }
            /*newData.bitFlags = is4engined ? 15 : 3;
            sc.SendDataOnSimObject(newData);*/
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class LeftEngineMaster : IButtonCallback<TcaAirbusQuadrant>
    {
        private readonly EnginerMasterAction action;
        public int GetButton() => TcaAirbusQuadrant.BUTTON_LEFT_ENGINE_MASTER;
        public void OnPress(ExtendedSimConnect sc) => action.perform(sc, true, true);
        public void OnRelease(ExtendedSimConnect sc) => action.perform(sc, true, false);
    }

    [Component, RequiredArgsConstructor]
    public partial class RightEngineMaster : IButtonCallback<TcaAirbusQuadrant>
    {
        private readonly EnginerMasterAction action;
        public int GetButton() => TcaAirbusQuadrant.BUTTON_RIGHT_ENGINE_MASTER;
        public void OnPress(ExtendedSimConnect sc) => action.perform(sc, false, true);
        public void OnRelease(ExtendedSimConnect sc) => action.perform(sc, false, false);
    }
}
