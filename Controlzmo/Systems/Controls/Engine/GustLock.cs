using Controlzmo.GameControllers;
using Controlzmo.SimConnectzmo;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls.Engine
{
    [Component, RequiredArgsConstructor]
    public partial class GustLock : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly InputEvents inputEvents;
        public int GetAxis() => UrsaMinorFighterR.AXIS_THROTTLE;

        public void OnChange(ExtendedSimConnect sc, double old, double @new)
        {
            if (@new == 0.0 || @new == 1.0)
                inputEvents.Send(sc, "HANDLING_HANDLING_LEVER_GUSTLOCK", @new);
        }
    }
}
