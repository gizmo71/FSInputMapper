﻿using Controlzmo.Hubs;
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

        protected override double? Value { set => MessageShown((base.Value = value) == 3.0); }

        private CancellationTokenSource? cancellationTokenSource;

        private void MessageShown(bool isLeverClimbDisplayed)
        {
            if (isLeverClimbDisplayed)
                if (cancellationTokenSource == null) {
                    cancellationTokenSource = new CancellationTokenSource();
                    Task.Delay(4_000, cancellationTokenSource.Token).ContinueWith(_ => {
                        if (!cancellationTokenSource.Token.IsCancellationRequested)
                            hubContext.Clients.All.Speak("Lever climb?");
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
