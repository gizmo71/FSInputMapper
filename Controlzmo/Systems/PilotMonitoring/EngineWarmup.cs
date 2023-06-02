using Controlzmo.Hubs;
using Controlzmo.Systems.EfisControlPanel;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

/* The logic for the AVAIL message is buried in N1.tsx:
    const [N1Percent] = useSimVar(`L:A32NX_ENGINE_N1:${engine}`, 'percent', 60);
    const [N1Idle] = useSimVar('L:A32NX_ENGINE_IDLE_N1', 'percent', 1000);
    const [engineState] = useSimVar(`L:A32NX_ENGINE_STATE:${engine}`, 'bool', 500);
    const availVisible = !!(N1Percent > Math.floor(N1Idle) && engineState === 2); // N1Percent sometimes does not reach N1Idle by .005 or so
Engine state is 0 when off, then 2 whilst starting and then 1 once started. After a shutdown there's a 4 and a 3 too!
Going from 2 to 1 on both engines seems to be a good enough trigger.
Sometimes we get numbers which seem to be 10 higher, which may be because the code thinks the sim is paused...*/
namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineWarmupData
    {
        [SimVar("L:A32NX_ENGINE_STATE:1", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 engine1State;
        [SimVar("L:A32NX_ENGINE_STATE:2", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 engine2State;
    };

    [Component]
    public class EngineWarmupListener : DataListener<EngineWarmupData>
    {
        private readonly JetBridgeSender jetbridge;
        private readonly Chrono1Event chronoEvent;
private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
//private readonly ILogger logging;

        private CancellationTokenSource? cancellationTokenSource;
        private bool isArmed = false;

        public EngineWarmupListener(IServiceProvider serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
            chronoEvent = serviceProvider.GetRequiredService<Chrono1Event>();
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
//logging = serviceProvider.GetRequiredService<ILogger<EngineWarmupListener>>();
        }

        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            var period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            if (!isOnGround)
                MaybeCancel();
        }

        public override void Process(ExtendedSimConnect simConnect, EngineWarmupData data)
        {
//logging.LogWarning($"{data.engine1State} and {data.engine2State} and {isArmed}");
            data.engine1State %= 10;
            data.engine2State %= 10;
            bool isOneRunningAndOneStarting = data.engine1State == 2 && data.engine2State == 1 || data.engine1State == 1 && data.engine2State == 2;
            bool areBothRunning = data.engine1State == 1 && data.engine2State == 1;
            if (isOneRunningAndOneStarting && !isArmed)
            {
                MaybeCancel();
                isArmed = true;
            }
            else if (areBothRunning && isArmed)
            {
                if (cancellationTokenSource == null) //TODO: we should not need this check any more
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken cancellationToken = cancellationTokenSource.Token;
                    Task.Delay(180_000, cancellationToken).ContinueWith(_ =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
hubContext.Clients.All.Speak("Engines warmed up");
                            jetbridge.Execute(simConnect, "1 (>L:A32NX_CABIN_READY)");
                        }
                        cancellationTokenSource = null;
                    });
                    simConnect.SendEvent(chronoEvent); // This is going to be annoying if it triggers too often
                }
else hubContext.Clients.All.Speak("How do we get to be armed but with a cancellation token source?");
                isArmed = false;
            }
            else if (data.engine1State == 0 || data.engine2State == 0 || data.engine1State == 3 || data.engine2State == 3)
            {
                isArmed = false;
                MaybeCancel();
            }
        }

        private void MaybeCancel()
        {
            if (cancellationTokenSource != null)
            {
hubContext.Clients.All.Speak("Cancel timer");
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }
        }
    }
}
