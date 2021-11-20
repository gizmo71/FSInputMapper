using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using SimConnectzmo;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuTrackFpa : LVar, IOnSimStarted
    {
        public FcuTrackFpa(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_TRK_FPA_MODE_ACTIVE";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
        public bool IsHdgVS { get => Value == 0; }
        public bool IsTrkFpa { get => Value == 1; }
    }

    [Component]
    public class FcuTrackFpaToggled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_TRK_FPA_TOGGLE_PUSH";
        public string GetId() => "trkFpaToggled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }
}
