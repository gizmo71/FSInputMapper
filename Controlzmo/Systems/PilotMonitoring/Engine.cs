using Controlzmo.SimConnectzmo;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.PilotMonitoring
{
    public abstract class EngineState : LVar
    {
        private readonly string lVarName;

        public EngineState(IServiceProvider serviceProvider, int engineNumber) : base(serviceProvider)
        {
            lVarName = "A32NX_ENGINE_STATE:" + engineNumber;
        }

        protected override string LVarName() => lVarName;
        protected override double Default() => -1.0;
        protected override int Milliseconds() => 4000;
    }

    [Component]
    public class Engine1State : EngineState
    {
        public Engine1State(IServiceProvider serviceProvider) : base(serviceProvider, 1) { }
    }

    [Component]
    public class Engine2State : EngineState
    {
        public Engine2State(IServiceProvider serviceProvider) : base(serviceProvider, 2) { }
    }
}
