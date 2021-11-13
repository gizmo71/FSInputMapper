using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuVsState : LVar, IOnSimStarted
    {
        public FcuVsState(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A320_NE0_FCU_STATE";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
/* Seems to be (based on https://github.com/flybywiresim/a32nx/blob/42e4134f9235ff0a1842edc10aad7c56a52b7989/flybywire-aircraft-a320-neo/html_ui/Pages/VCockpit/Instruments/Airliners/FlyByWire_A320_Neo/FCU/A320_Neo_FCU.js):
 * 0 'idle' when in ALT (-----), which also triggers "managed" to be set; then for VS:
 * 1 when pushed to level ( 00oo);
 * 2 for selecting V/S and 3 after pulling (both +/-00oo).
 * In FPA, the number is always shown except for condition 0. */
        public bool IsIdle { get => Value == 0; }
    }

    [Component]
    public class FcuVsSelected : LVar, IOnSimStarted
    {
        public FcuVsSelected(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOPILOT_VS_SELECTED";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
    }

    [Component]
    public class FcuFpaSelected : LVar, IOnSimStarted
    {
        public FcuFpaSelected(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOPILOT_FPA_SELECTED";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
    }

    [Component]
    public class FcuVsPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuVsPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuVsPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_VS_PULL)");
    }

    [Component]
    public class FcuVsPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuVsPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuVsPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_VS_PUSH)");
    }

    [Component]
    public class FcuVsDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;
        private readonly FcuVsSelected fcuVsSelected;

        public FcuVsDelta(IServiceProvider sp)
        {
            sender = sp.GetRequiredService<JetBridgeSender>();
            fcuVsSelected = sp.GetRequiredService<FcuVsSelected>();
        }

        public string GetId() => "fcuVsDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_VS_DEC" : "A32NX.FCU_VS_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short)Math.Sign(value);
            }
            fcuVsSelected.Request(simConnect);
        }
    }
}
