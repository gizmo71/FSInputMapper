using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

/* The logic for the AVAIL message is buried in N1.tsx:
    const [N1Percent] = useSimVar(`L:A32NX_ENGINE_N1:${engine}`, 'percent', 60);
    const [N1Idle] = useSimVar('L:A32NX_ENGINE_IDLE_N1', 'percent', 1000);
    const [engineState] = useSimVar(`L:A32NX_ENGINE_STATE:${engine}`, 'bool', 500);
    const availVisible = !!(N1Percent > Math.floor(N1Idle) && engineState === 2); // N1Percent sometimes does not reach N1Idle by .005 or so
Engine state is 0 when off, then 2 whilst starting and then 1 once started. After a shutdown there's a 4 and a 3 too!
Going from 2 to 1 on both engines would probably be a good enough trigger.*/
namespace Controlzmo.Systems.PilotMonitoring
{
    [Component]
    public class EngineWarmup : IOnSimStarted
    {
        private readonly Engine1State engine1State;
        private readonly Engine2State engine2State;

        private readonly SimConnectHolder scHolder;
        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public EngineWarmup(IServiceProvider serviceProvider)
        {
            engine1State = serviceProvider.GetRequiredService<Engine1State>();
            engine2State = serviceProvider.GetRequiredService<Engine2State>();

            engine1State.PropertyChanged += OnPropertyChanged;
            engine2State.PropertyChanged += OnPropertyChanged;

            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
            scHolder = serviceProvider.GetRequiredService<SimConnectHolder>();
        }

        public void OnStarted(ExtendedSimConnect simConnect)
        {
            engine1State.Request(simConnect);
            engine2State.Request(simConnect);
        }

        private CancellationTokenSource? cancellationTokenSource;

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            bool areBothRunning = engine1State == 1.0 && engine2State == 1.0;
            if (areBothRunning)
                if (cancellationTokenSource == null)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    Task.Delay(180_000, cancellationTokenSource.Token).ContinueWith(_ => {
                        if (!cancellationTokenSource.Token.IsCancellationRequested)
                            jetbridge.Execute(scHolder.SimConnect!, "1 (>L:A32NX_CABIN_READY)");
                    });
                }
                else if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource = null;
                }
        }
    }
}
