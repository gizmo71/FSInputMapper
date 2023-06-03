using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AutothrustModeMessageData
    {
        [SimVar("L:A32NX_AUTOTHRUST_MODE_MESSAGE", "number", SIMCONNECT_DATATYPE.INT32, 1f)]
        public Int32 modeMessage;
    };

    [Component]
    public class LeverClimb : DataListener<AutothrustModeMessageData>, IOnSimStarted
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public LeverClimb(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        private CancellationTokenSource? cancellationTokenSource;

        public override void Process(ExtendedSimConnect simConnect, AutothrustModeMessageData data)
        {
            if (data.modeMessage == 3) {
                if (cancellationTokenSource == null) {
                    cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken cancellationToken = cancellationTokenSource.Token;
                    Task.Delay(5_000, cancellationToken).ContinueWith(_ => {
                        if (!cancellationToken.IsCancellationRequested)
                            hubContext.Clients.All.Speak("Lever climb?");
//else hubContext.Clients.All.Speak("never mind");
                        cancellationTokenSource = null;
                    });
//hubContext.Clients.All.Speak("er...");
                }
//else hubContext.Clients.All.Speak("ditto");
            }
            else
            {
//hubContext.Clients.All.Speak("oh");
                cancellationTokenSource?.Cancel();
            }
        }
    }
}
