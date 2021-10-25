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
        private readonly BlockingCollection<Action<ExtendedSimConnect>> _jobs = new BlockingCollection<Action<ExtendedSimConnect>> ();
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

        public void Enqueue(Action<ExtendedSimConnect> job)
        {
            _jobs.TryAdd(job);
        }

        private static readonly TimeSpan dequeueTimeout = TimeSpan.FromSeconds(10);
        private void OnStart()
        {
            Action<ExtendedSimConnect>? job;
            for (;;)
            {
                if (_jobs.TryTake(out job, dequeueTimeout))
                {
                    _logging.LogInformation($"Serializing action {job}");
                    try
                    {
                        job.Invoke(holder.SimConnect!);
                    }
                    catch (Exception e)
                    {
                        _logging.LogError("Invocation failed: {0}", e);
                    }
                    Thread.Sleep(50);
                }
                else
                    _logging.LogInformation("No serialized actions to perform");
            }
        }
    }
}
