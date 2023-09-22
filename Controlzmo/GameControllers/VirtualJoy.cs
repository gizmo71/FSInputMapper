using CoreDX.vJoy.Wrapper;
using System;

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
}
