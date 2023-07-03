using Controlzmo.Hubs;
using SimConnectzmo;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuTrackFpaToggled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_TRK_FPA_TOGGLE_PUSH";
        public string GetId() => "trkFpaToggled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }
}
