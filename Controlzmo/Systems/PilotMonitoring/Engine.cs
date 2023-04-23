using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineN1Data
    {
        [SimVar("L:A32NX_ENGINE_N1:1", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.1f)]
        public Double engine1N1;
        [SimVar("L:A32NX_ENGINE_N1:2", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.1f)]
        public Double engine2N1;
    };

    [Component]
    public class EngineN1Listener : DataListener<EngineN1Data>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public EngineN1Listener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, EngineN1Data data)
        {
            //TODO: replace the stuff in ThrustSet with traditional data listeners.
        }
    }
}
