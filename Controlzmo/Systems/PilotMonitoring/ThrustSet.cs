using System;
using System.ComponentModel;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    public abstract class SwitchableLVar : LVar
    {
        public SwitchableLVar(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override int Milliseconds() => 4000;
    }

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

    [Component]
    public class AutothrustMode : SwitchableLVar, IOnSimConnection
    {
        private readonly ThrustLeverN1 thrustLever1N1;
        private readonly ThrustLeverN1 thrustLever2N1;
        private readonly EngineN1 engine1N1;
        private readonly EngineN1 engine2N1;

        private readonly SimConnectHolder scHolder;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public AutothrustMode(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            thrustLever1N1 = serviceProvider.GetRequiredService<ThrustLever1N1>();
            thrustLever2N1 = serviceProvider.GetRequiredService<ThrustLever2N1>();
            engine1N1 = serviceProvider.GetRequiredService<Engine1N1>();
            engine2N1 = serviceProvider.GetRequiredService<Engine2N1>();

            engine1N1.PropertyChanged += OnPropertyChanged;
            engine2N1.PropertyChanged += OnPropertyChanged;

            scHolder = serviceProvider.GetRequiredService<SimConnectHolder>();
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();

            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }

        protected override string LVarName() => "A32NX_AUTOTHRUST_MODE";
        protected override double Default() => -1.0;

        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            Request(simConnect, isOnGround ? 1000 : 0);
            if (!isOnGround) {
                engine1N1.Request(simConnect, 0);
                engine2N1.Request(simConnect, 0);
            }
        }

        public void OnConnection(ExtendedSimConnect simConnect)
        {
            // Do nothing, but get me created. This does not seem right compared to other things.
        }

        protected override double? Value
        {
            set
            {
                if (base.Value != value)
                {
                    base.Value = value;
                    var sc = scHolder.SimConnect!;
                    thrustLever1N1.Request(sc);
                    thrustLever2N1.Request(sc);
                    int period = IsTakeOffPower() ? 1000 : 0;
                    engine1N1.Request(sc, period);
                    engine2N1.Request(sc, period);
                }
            }
        }

        private const double TOGA = 1;
        private const double FLEX = 3;
        private Boolean IsTakeOffPower() => Value == TOGA || Value == FLEX;

        private Boolean isCalled = false;

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (IsTakeOffPower())
            {
                if (!isCalled && isSet(engine1N1, thrustLever1N1) && isSet(engine2N1, thrustLever2N1))
                {
                    hubContext.Clients.All.Speak($"thrust set");
                    isCalled = true;
                }
            }
            else
                isCalled = false;
        }

        private Boolean isSet(EngineN1 engine, ThrustLeverN1 lever)
        {
            return lever > 75.0 && engine >= lever - 0.1;
        }
    }
}
