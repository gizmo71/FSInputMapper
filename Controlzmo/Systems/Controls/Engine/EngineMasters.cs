using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.Timers;

namespace Controlzmo.Systems.Controls.Engine
{
    // UI: SET ENGINE n FUEL VALVE
    [Component] public class FuelSystemValveCloseEvent : IEvent { public string SimEvent() => "FUELSYSTEM_VALVE_CLOSE"; }
    [Component] public class FuelSystemValveOpenEvent : IEvent { public string SimEvent() => "FUELSYSTEM_VALVE_OPEN"; }

    [Component, RequiredArgsConstructor]
    public partial class EnginerMasterAction
    {
        private readonly FuelSystemValveCloseEvent fuelSystemValveClose;
        private readonly FuelSystemValveOpenEvent fuelSystemValveOpen;
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
            else if (sc.IsIni330 || sc.IsIni321) // Maybe also other INIs? Obviously not the A400...
            {
                var engineId = isLeft ? 1 : 2;
                sender.Execute(sc, $"{value} (>L:INI_MIXTURE_RATIO{engineId}_HANDLE)");
                return;
            }

/*TODO: the A400M has three positions for each switch, "off", "feather" (used during startup), and "run" (one AVAIL has been shown).
  We should go to "feather" initially (as we do, in fact), but then automatically switch to "run" once it's "up". */
            var is4engined = sc.IsA380X || sc.IsB748 || sc.IsIni400M;
            var first = isLeft ? 1u : (is4engined ? 3u : 2u);
            var last = first + (is4engined ? 1u : 0u);
            var delay = 1;
            for (var engine = first; engine <= last; ++engine) {
                DelayedEngineMasterCommand(sc, isOn ? fuelSystemValveOpen : fuelSystemValveClose, engine, delay);
                delay += 3000;
            }
        }

        private void DelayedEngineMasterCommand(ExtendedSimConnect sc, IEvent _event, uint engine, int delay)
        {
            var timer = new Timer(delay);
            timer.AutoReset = false;
            timer.Elapsed += (object? source, ElapsedEventArgs e) => {
                sc.SendEventEx1(_event, engine);
                timer.Dispose(); // Probably don't need to do this. timer == source
            };
            timer.Start();
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
