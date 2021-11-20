using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
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
    public class FcuVsPulled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_PULL";
        public string GetId() => "fcuVsPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuVsPushed : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_PUSH";
        public string GetId() => "fcuVsPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuVsInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_INC";
    }

    [Component]
    public class FcuVsDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_DEC";
    }

    [Component]
    public class FcuVsDelta : ISettable<Int16>
    {
        private readonly FcuVsInc inc;
        private readonly FcuVsDec dec;

        public FcuVsDelta(IServiceProvider sp)
        {
            inc = sp.GetRequiredService<FcuVsInc>();
            dec = sp.GetRequiredService<FcuVsDec>();
        }

        public string GetId() => "fcuVsDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            while (value != 0)
            {
                simConnect.SendEvent(value < 0 ? dec : inc);
                value -= (short)Math.Sign(value);
            }
//TODO: in the real FCU, when turning quickly, it takes *two* clicks to change by 100 ft/min V/S.
        }
    }
}
