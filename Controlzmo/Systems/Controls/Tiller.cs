using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

/*TODO: can we somehow disconnect the tiller in the ATR once on the take off roll, and leave it off until after landing?
  L:MSATR_Switch_NW_STEERING_cover and L:MSATR_NW_STEERING are implicated... there are some B: events, too. */
namespace Controlzmo.Systems.Controls
{
    [Component] public class TillerEvent : IEvent { public string SimEvent() => "AXIS_STEERING_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class Tiller : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly TillerEvent _event;
        public int GetAxis() => UrsaMinorFighterR.AXIS_TWIST;
        public void OnChange(ExtendedSimConnect sc, double _, double @new) => sc.SendEvent(_event, 16383 - (int) (32767.0 * @new));
    }

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
            var variable =  "notWorkingA32NX_TILLER_PEDAL_DISCONNECT";
            if (sc.IsFenix) variable = "S_FC_CAPT_TILLER_PEDAL_DISCONNECT";
            else if (sc.IsIniBuilds) variable = "INI_TILLER_DISC";
            sender.Execute(sc, $"{value} (>L:{variable})");
        }
    }
}
