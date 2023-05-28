using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
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
Going from 2 to 1 on both engines seems to be a good enough trigger.*/
namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineWarmupData
    {
        [SimVar("L:A32NX_ENGINE_STATE:1", "number", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 engine1State;
        [SimVar("L:A32NX_ENGINE_STATE:2", "number", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 engine2State;
    };

    [Component]
    public class EngineWarmupListener : DataListener<EngineWarmupData>, IRequestDataOnOpen
    {
        private readonly JetBridgeSender jetbridge;

        private CancellationTokenSource? cancellationTokenSource;

        public EngineWarmupListener(IServiceProvider serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, EngineWarmupData data)
        {
            bool areBothRunning = data.engine1State == 1.0 && data.engine2State == 1.0;
            if (areBothRunning)
                if (cancellationTokenSource == null)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken cancellationToken = cancellationTokenSource.Token;
                    Task.Delay(180_000, cancellationToken).ContinueWith(_ => {
                        if (!cancellationToken.IsCancellationRequested)
                            jetbridge.Execute(simConnect, "1 (>L:A32NX_CABIN_READY)");
                        cancellationTokenSource = null;
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
