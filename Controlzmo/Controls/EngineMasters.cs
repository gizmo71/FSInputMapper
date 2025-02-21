using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;

namespace Controlzmo.Controls
{
    [Component, RequiredArgsConstructor]
    public partial class EnginerMasterAction
    {
        private readonly JetBridgeSender sender;
        internal void perform(ExtendedSimConnect sc, Boolean isLeft, Boolean isOn)
        {
            var value = isOn ? 1 : 0;
            var valve_action = isOn ? "OPEN" : "CLOSE";
/*TODO: the A400M has three positions for each switch, "off", "feather" (used during startup), and "run" (one AVAIL has been shown).
  We should go to "feather" initially (as we do, in fact), but then automatically switch to "run" once it's "up". */
            var is4engined = sc.IsA380X || sc.IsB748 || sc.IsIni400M;
            var first = isLeft ? 1 : (is4engined ? 3 : 2);
            var last = first + (is4engined ? 1 : 0);
/*TODO: seems to have stopped working for the A380X, at least in MSFS2024 :-(
Toggling on only toggles 1/2, as if we're not 4-engined.
Toggling off turns both off but only sort of... :-o
*/
            for (var engine = first; engine <= last; ++engine)
                sender.Execute(sc, $"{value} (>K:ENGINE_MASTER_{engine}_SET)" // UI: SET ENGINE MASTER 1
                    + $" {engine} (>K:FUELSYSTEM_VALVE_{valve_action})" // UI: SET ENGINE n FUEL VALVE
                    + $" {value} (>K:STARTER{engine}_SET)"); // UI: SET STARTER 1
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
