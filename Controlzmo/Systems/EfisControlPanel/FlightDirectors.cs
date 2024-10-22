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
            else
                sender.Execute(sc, "1 (>K:TOGGLE_FLIGHT_DIRECTOR)");
        }
    }
}
