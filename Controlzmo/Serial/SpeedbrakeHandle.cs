using System;
using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimConnectzmo;

namespace Controlzmo.Serial
{
    [Component]
    public class SpeedbrakeHandle : ISettable<Int16?>
    {
        private readonly ILogger _logger;

        public SpeedbrakeHandle(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<SpeedbrakeHandle>>();
        }

        public string GetId() => "speedBrakeHandle";

        public void SetInSim(ExtendedSimConnect simConnect, Int16? value)
        {
            _logger.LogTrace($"Imaginary 'speedBrakeHandle' set to {value}");
        }
    }
}
