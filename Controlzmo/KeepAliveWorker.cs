using System;
using System.ComponentModel;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Controlzmo
{
    public abstract class KeepAliveWorker
    {
        private readonly ILogger _logger;

        private BackgroundWorker? bw;

        protected KeepAliveWorker(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<KeepAliveWorker>>();

            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += (object? sender, ElapsedEventArgs args) => EnsureConnectionIfPossible();
            timer.Start();
        }

        private void EnsureConnectionIfPossible()
        {
            if (bw == null)
            {
                _logger.LogDebug("Starting background worker for " + GetType());
                bw = new BackgroundWorker() { WorkerSupportsCancellation = true };
                bw.DoWork += DonkeyOuter;
                bw.RunWorkerAsync();
            }
            else
                _logger.LogTrace("Existing background worker for " + GetType());
        }

        private void DonkeyOuter(object? sender, DoWorkEventArgs args)
        {
            try
            {
                OnStart(sender, args);
                while (!IsCancellationPending())
                    OnLoop(sender, args);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception running " + GetType());
            }
            finally
            {
                try
                {
                    OnStop(sender, args);
                }
                finally
                {
                    bw = null;
                }
            }
        }

        protected abstract void OnStart(object? sender, DoWorkEventArgs args);
        protected abstract void OnLoop(object? sender, DoWorkEventArgs args);
        protected abstract void OnStop(object? sender, DoWorkEventArgs args);

        protected Boolean IsCancellationPending() => bw!.CancellationPending;
    }
}
