using System;
using System.ComponentModel;
using Controlzmo.SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    public abstract class ThrustLeverN1 : LVar
    {
        private readonly string lVarName; // Or A32NX_AUTOTHRUST_N1_COMMANDED?

        public ThrustLeverN1(IServiceProvider serviceProvider, int engineNumber) : base(serviceProvider)
        {
            lVarName = "A32NX_AUTOTHRUST_TLA_N1:" + engineNumber;
        }

        protected override string LVarName() => lVarName;
        protected override int Milliseconds() => 0;
        protected override double Default() => -1.0;
    }

    [Component]
    public class ThrustLever1N1 : ThrustLeverN1
    {
        public ThrustLever1N1(IServiceProvider serviceProvider) : base(serviceProvider, 1) { }
    }

    [Component]
    public class ThrustLever2N1 : ThrustLeverN1
    {
        public ThrustLever2N1(IServiceProvider serviceProvider) : base(serviceProvider, 2) { }
    }
}
