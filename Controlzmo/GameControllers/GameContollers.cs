using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;

namespace Controlzmo.GameControllers
{
    [Component]
    public class GameControllers : CreateOnStartup
    {
        private readonly ILogger _log;
        private readonly IEnumerable<IGameController> controllers;

        public GameControllers(IServiceProvider sp)
        {
            _log = sp.GetRequiredService<ILogger<GameControllers>>();
            controllers = sp.GetServices<IGameController>();
            RawGameController.RawGameControllerAdded += Added;
            RawGameController.RawGameControllerRemoved += Removed;
            RawGameController.RawGameControllers.ToList().ForEach(c => Added(null, c));
        }

        private void Added(object? sender, RawGameController c)
        {
            _log.LogDebug($"Controller {c.HardwareVendorId} {c.HardwareProductId} = {c.DisplayName} added");
            _log.LogDebug($"\thas {c.ButtonCount} buttons, {c.AxisCount} axes, {c.SwitchCount} switches");
            foreach (var gc in controllers)
                gc.Offer(c);
        }

        private void Removed(object? sender, RawGameController c)
        {
            // Is there actually any reason to do anything?
        }
    }
}
