using System;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    public abstract class SwitchableLVar : LVar
    {
        private int period = 4000;
        public SwitchableLVar(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override int Milliseconds() => period;

        public void Request(ExtendedSimConnect simConnect, int period)
        {
            this.period = period;
            Request(simConnect);
        }
    }

    [Component]
    public class ThrustLever1N1 : LVar
    {
        public ThrustLever1N1(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOTHRUST_TLA_N1:1"; // Or A32NX_AUTOTHRUST_N1_COMMANDED?
        protected override int Milliseconds() => 0;
        protected override double Default() => -1.0;
    }

    [Component]
    public class Engine1N1 : SwitchableLVar
    {
        private readonly ThrustLever1N1 thrustLever1N1;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly SimConnectHolder scHolder;

        public Engine1N1(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            thrustLever1N1 = serviceProvider.GetRequiredService<ThrustLever1N1>();
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            scHolder = serviceProvider.GetRequiredService<SimConnectHolder>();
        }

        protected override string LVarName() => "A32NX_ENGINE_N1:1";
        protected override double Default() => -1.0;

        protected override double? Value
        {
            set
            {
                if (base.Value != value)
                {
                    base.Value = value;
                    if (Milliseconds() != 0 && thrustLever1N1 > 60.0 && value >= thrustLever1N1 - 0.1)
                    {
                        hubContext.Clients.All.Speak($"thrust set");
                        Request(scHolder.SimConnect!, 0);
                    }
                }
            }
        }
    }

    [Component]
    public class AutothrustMode : SwitchableLVar, IOnSimConnection
    {
        private const double TOGA = 1;
        private const double FLEX = 3;

        private readonly ThrustLever1N1 thrustLever1N1;
        private readonly Engine1N1 engine1N1;
        private readonly SimConnectHolder scHolder;

        public AutothrustMode(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            thrustLever1N1 = serviceProvider.GetRequiredService<ThrustLever1N1>();
            engine1N1 = serviceProvider.GetRequiredService<Engine1N1>();
            scHolder = serviceProvider.GetRequiredService<SimConnectHolder>();

            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }

        protected override string LVarName() => "A32NX_AUTOTHRUST_MODE";
        protected override double Default() => -1.0;

        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            Request(simConnect, isOnGround ? 1000 : 0);
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
                    engine1N1.Request(sc, (value == TOGA || value == FLEX) ? 1000 : 0);
                }
            }
        }
    }
}
