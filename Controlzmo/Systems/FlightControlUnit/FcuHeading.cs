using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Linq;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuHeadingManaged : LVar, IOnSimStarted
    {
        public FcuHeadingManaged(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_FCU_HDG_MANAGED_DOT";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
        public bool IsManaged { get => Value == 1; }
    }

    [Component] // Gets confused when flipping between TRK and HDG...
    public class FcuHeadingSelected : LVar, IOnSimStarted
    {
        public FcuHeadingSelected(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOPILOT_HEADING_SELECTED";
        protected override double Default() => 999;
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
    }

    [Component] // ... so we need this to definitively decide.
    public class FcuHeadingDashes : LVar, IOnSimStarted
    {
        public FcuHeadingDashes(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_FCU_HDG_MANAGED_DASHES";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
        public bool IsDashes { get => Value == 1; }
    }

    [Component]
    public class FcuHeadingPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuHeadingPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuHeadingPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_HDG_PULL)");
    }

    [Component]
    public class FcuHeadingPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuHeadingPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuHeadingPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_HDG_PUSH)");
    }

    [Component]
    public class FcuHeadingInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_HDG_INC";
    }

    [Component]
    public class FcuHeadingDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_HDG_DEC";
    }

    [Component]
    public class FcuHeadingDelta : ISettable<Int16>
    {
        private readonly FcuHeadingInc inc;
        private readonly FcuHeadingDec dec;

        public FcuHeadingDelta(IServiceProvider sp)
        {
            inc = sp.GetRequiredService<FcuHeadingInc>();
            dec = sp.GetRequiredService<FcuHeadingDec>();
        }

        public string GetId() => "fcuHeadingDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            while (value != 0)
            {
                simConnect.SendEvent(value < 0 ? dec : inc);
                value -= (short)Math.Sign(value);
            }
        }
    }
}
