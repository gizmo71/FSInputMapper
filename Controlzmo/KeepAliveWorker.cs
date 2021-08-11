using System;
using System.ComponentModel;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Controlzmo
{
    public abstract class KeepAliveWorker
    {
        private readonly ILogger<KeepAliveWorker> _logger;

        private BackgroundWorker? bw;

        protected KeepAliveWorker(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<KeepAliveWorker>>();

            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += (object sender, ElapsedEventArgs args) => EnsureConnectionIfPossible();
            timer.Start();
        }

        private void EnsureConnectionIfPossible()
        {
            if (bw == null)
            {
_logger.LogDebug("Starting background worker for " + GetType());
                bw = new BackgroundWorker() { WorkerSupportsCancellation = true };
                bw.DoWork += Donkey;
                bw.RunWorkerAsync();
            }
else _logger.LogDebug("Existing background worker for " + GetType());
        }

        private void DonkeyOuter(object? sender, DoWorkEventArgs args)
        {
            try
            {
                Donkey(sender, args);
            }
            finally
            {
                bw = null;
            }
        }

        protected abstract void Donkey(object? sender, DoWorkEventArgs args);

        protected Boolean IsCancellationPending() => bw!.CancellationPending;
    }
}
