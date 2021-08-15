using System;
using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimConnectzmo;

namespace Controlzmo.Serial
{
    [Component]
    public class ApuMasterButton : ISettable<Int16?>
    {
        private readonly ILogger _logger;

        public ApuMasterButton(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<ApuMasterButton>>();
        }

        public string GetId() => "apuMasterPressed";

        public void SetInSim(ExtendedSimConnect simConnect, Int16? value)
        {
            _logger.LogTrace($"APU master button pressed");
        }
    }

    [Component]
    public class ApuStartButton : ISettable<Int16?>
    {
        private readonly ILogger _logger;

        public ApuStartButton(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<ApuMasterButton>>();
        }

        public string GetId() => "apuStartPressed";

        public void SetInSim(ExtendedSimConnect simConnect, Int16? value)
        {
            _logger.LogTrace($"APU start button pressed");
        }
    }
}
