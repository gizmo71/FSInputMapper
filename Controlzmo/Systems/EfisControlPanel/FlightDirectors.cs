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
            if (sc.IsFenix)
                for (int i = 2; i-- > 0; )
                    sender.Execute(sc, $"{i} (>L:S_FCU_EFIS1_FD)");
            else if (sc.IsIniBuilds)
                sender.Execute(sc, $"(L:INI_FD1_ON) ! d (>L:INI_FD1_ON)");
            else
//TODO: this is a bit odd in the A380X... there's only a single button.
                sender.Execute(sc, "1 (>K:TOGGLE_FLIGHT_DIRECTOR)");
        }
    }
}
