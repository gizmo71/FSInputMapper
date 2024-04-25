using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using SimConnectzmo;

namespace Controlzmo
{
    [Component]
    public class SerializedExecutor : CreateOnStartup
    {
        private readonly BlockingCollection<Func<ExtendedSimConnect, Boolean>> _jobs = new BlockingCollection<Func<ExtendedSimConnect, Boolean>> ();
        private readonly ILogger _logging;
        private readonly SimConnectHolder holder;

        public SerializedExecutor(IServiceProvider sp)
        {
            _logging = sp.GetRequiredService<ILogger<SerializedExecutor>>();
            holder = sp.GetRequiredService<SimConnectHolder>();

            var thread = new Thread(new ThreadStart(OnStart));
            thread.IsBackground = true;
            thread.Start();
        }

        public void Enqueue(Func<ExtendedSimConnect, Boolean> job)
        {
            _jobs.TryAdd(job);
        }

        private static readonly TimeSpan dequeueTimeout = TimeSpan.FromSeconds(10);
        private void OnStart()
        {
            Func<ExtendedSimConnect, Boolean>? job;
            for (;;)
            {
                if (_jobs.TryTake(out job, dequeueTimeout))
                {
                    _logging.LogInformation($"Serializing action {job}");
                    try
                    {
                        if (!job.Invoke(holder.SimConnect!))
                            continue; // Did nothing, don't need to pause.
                    }
                    catch (Exception e)
                    {
                        _logging.LogError("Invocation failed: {0}", e);
                    }
                    Thread.Sleep(250);
                }
                else
                    _logging.LogInformation("No serialized actions to perform");
            }
        }
    }
}
