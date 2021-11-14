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
    public class FcuHeadingDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuHeadingDelta(IServiceProvider sp)
        {
            sender = sp.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "fcuHeadingDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_HDG_DEC" : "A32NX.FCU_HDG_INC";
            while (value != 0)
            {
                var repeat = Math.Min(Math.Abs(value), (Int16)5);
                sender.Execute(simConnect, Enumerable.Repeat($"0 (>K:{eventCode})", repeat).Aggregate((l, r) => l + " " + r));
                value -= (Int16)(Math.Sign(value) * repeat);
            }
            //TODO: use A32NX.FCU_HDG_SET, 0 to 359
            //TODO: can we use :4 g4 type constructs in RPN to form loops and avoid multiple sends here?
        }
    }
}
