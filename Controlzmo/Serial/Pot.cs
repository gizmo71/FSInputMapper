using System;
using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimConnectzmo;

namespace Controlzmo.Serial
{
    [Component]
    public class Pot : ISettable<Int16?>
    {
        private readonly ILogger _logger;

        public Pot(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<Pot>>();
        }

        public string GetId() => "pot";

        public void SetInSim(ExtendedSimConnect simConnect, Int16? value)
        {
            _logger.LogTrace($"Imaginary 'pot' set to {value}");
        }
    }
}
