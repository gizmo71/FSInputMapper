using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
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
    public class FcuTrackFpaToggled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuTrackFpaToggled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "trkFpaToggled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_TRK_FPA_TOGGLE_PUSH)");
    }
}
