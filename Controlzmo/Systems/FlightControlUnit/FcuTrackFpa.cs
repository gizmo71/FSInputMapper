using Controlzmo.SimConnectzmo;
using SimConnectzmo;
using System;

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
}
