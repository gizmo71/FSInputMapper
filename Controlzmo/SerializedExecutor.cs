using System;
using System.Collections.Concurrent;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;

namespace Controlzmo
{
    [Component]
    public class SerializedExecutor : CreateOnStartup
    {
        private readonly BlockingCollection<Action> _jobs = new BlockingCollection<Action>();
        private readonly ILogger _logging;

        public SerializedExecutor(IServiceProvider sp)
        {
            _logging = sp.GetRequiredService<ILogger<SerializedExecutor>>();

            var thread = new Thread(new ThreadStart(OnStart));
            thread.IsBackground = true;
            thread.Start();
        }

        public void Enqueue(Action job)
        {
            _jobs.TryAdd(job);
        }

        private static readonly TimeSpan dequeueTimeout = TimeSpan.FromSeconds(10);
        private void OnStart()
        {
            Action? job;
            for (;;)
            {
                if (_jobs.TryTake(out job, dequeueTimeout))
                {
                    _logging.LogInformation($"Serializing action {job}");
                    job.Invoke();
                    Thread.Sleep(50);
                }
                else
                    _logging.LogInformation("No serialized actions to perform");
            }
        }
    }
}
