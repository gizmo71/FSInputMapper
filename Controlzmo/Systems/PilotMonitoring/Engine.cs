using System;
using System.ComponentModel;

namespace Controlzmo.Systems.PilotMonitoring
{

    public abstract class EngineN1 : SwitchableLVar
    {
        private readonly string lVarName;

        public EngineN1(IServiceProvider serviceProvider, int engineNumber) : base(serviceProvider)
        {
            lVarName = "A32NX_ENGINE_N1:" + engineNumber;
        }

        protected override string LVarName() => lVarName;
        protected override double Default() => -1.0;
    }

    [Component]
    public class Engine1N1 : EngineN1
    {
        public Engine1N1(IServiceProvider serviceProvider) : base(serviceProvider, 1) { }
    }

    [Component]
    public class Engine2N1 : EngineN1
    {
        public Engine2N1(IServiceProvider serviceProvider) : base(serviceProvider, 2) { }
    }
}
