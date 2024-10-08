﻿using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls
{
    [Component]
    [RequiredArgsConstructor]
    public partial class PedalDisconnect : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_PINKY;

        public virtual void OnPress(ExtendedSimConnect sc) {
            setTillerPedal(sc, true);
        }

        public virtual void OnRelease(ExtendedSimConnect sc) {
            setTillerPedal(sc, false);
        }

        private void setTillerPedal(ExtendedSimConnect sc, bool disconnected)
        {
            var value = disconnected ? 1 : 0;
//TODO: A32NX doesn't appear to support an LVar, despite what the docs suggest.
            var variable = sc.IsFenix ? "S_FC_CAPT_TILLER_PEDAL_DISCONNECT" : "notWorkingA32NX_TILLER_PEDAL_DISCONNECT";
            sender.Execute(sc, $"{value} (>L:{variable})");
        }
    }
}
