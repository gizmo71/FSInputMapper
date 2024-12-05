using Controlzmo.Hubs;
using CoreDX.vJoy.Wrapper;
using SimConnectzmo;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Controlzmo.GameControllers
{
    [Component]
    public class VirtualJoy : IDisposable, ISettable<uint>
    {
        private readonly VJoyControllerManager vJoyManager;
        private readonly IVJoyController _controller;

        public VirtualJoy()
        {
            vJoyManager = VJoyControllerManager.GetManager();
            _controller = vJoyManager.AcquireController(1);
        }

        public IVJoyController getController() => _controller;

        public void Dispose()
        {
            vJoyManager.RelinquishController(_controller);
            vJoyManager.Dispose();
        }

        public string GetId() => "vJoyClick";
        public void SetInSim(ExtendedSimConnect simConnect, uint buttonId) => _controller.QuickClick(buttonId);
    }

    public static class IVJoyControllerExtensions
    {
        private static readonly CancellationToken token = new CancellationToken();

        public static Task<bool> QuickClick(this IVJoyController controller, uint button)
        {
            return controller.ClickButtonAsync(button, 250, token);
        }
    }
}
