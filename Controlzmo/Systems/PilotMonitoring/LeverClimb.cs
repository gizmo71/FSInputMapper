using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Controlzmo.Systems.PilotMonitoring
{
    [Component]
    public class LeverClimb : LVar, IOnSimConnection
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public LeverClimb(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        protected override string LVarName() => "A32NX_AUTOTHRUST_MODE_MESSAGE";

        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set => MessageShown(base.Value = value); }

        private CancellationTokenSource? cancellationTokenSource;

        private void MessageShown(double? value)
        {
            if (value == 3.0) {
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
//else if (value > 0.0) hubContext.Clients.All.Speak("whatevs");
        }
    }
}
