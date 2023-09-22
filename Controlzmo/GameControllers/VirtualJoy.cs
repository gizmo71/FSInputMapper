using CoreDX.vJoy.Wrapper;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Controlzmo.GameControllers
{
    [Component]
    public class VirtualJoy : IDisposable
    {
        private readonly VJoyControllerManager vJoyManager;
        private readonly IVJoyController controller1;

        public VirtualJoy()
        {
            vJoyManager = VJoyControllerManager.GetManager();
            controller1 = vJoyManager.AcquireController(1);
        }

        public IVJoyController getController() => controller1;

        public void Dispose()
        {
            vJoyManager.RelinquishController(controller1);
            vJoyManager.Dispose();
        }
    }

    public static class IVJoyControllerExtensions
    {
        private static readonly CancellationToken token = new CancellationToken();

        public static Task<bool> QuickClick(this IVJoyController controller, uint button)
        {
            return controller.ClickButtonAsync(button, 100, token);
        }
    }
}
