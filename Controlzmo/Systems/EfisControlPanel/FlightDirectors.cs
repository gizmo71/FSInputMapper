using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.EfisControlPanel
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FlightDirectorToggle : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetButton() => UrsaMinorFighterR.BUTTON_LARGER_ROUND;

        public virtual void OnPress(ExtendedSimConnect sc)
        {
//TODO: this is a bit odd in the A380X... it seems to work, but there's no visible button. Is it tied to the VV button?
            if (sc.IsFenix)
                for (int i = 2; i-- > 0; )
                    sender.Execute(sc, $"{i} (>L:S_FCU_EFIS1_FD)");
            else
                sender.Execute(sc, "1 (>K:TOGGLE_FLIGHT_DIRECTOR)");
        }
    }
}
