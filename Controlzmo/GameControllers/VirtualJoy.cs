using Controlzmo.Hubs;
using CoreDX.vJoy.Wrapper;
using SimConnectzmo;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Controlzmo.GameControllers
{
    public enum VJoyButton : uint
    {
// These are the numbers we send and are listed in the in-game controller mappings; the actual numbers in the control mapping exports are one lower.
        RUDDER_TRIM_LEFT = 20,
        RUDDER_TRIM_RIGHT = 21,
        //INSTRUMENT_VIEW_NEXT = 89,
        INSTRUMENT_VIEW_10 = 90,
        INSTRUMENT_VIEW_1 = 91,
        INSTRUMENT_VIEW_2 = 92,
        INSTRUMENT_VIEW_3 = 93,
        INSTRUMENT_VIEW_4 = 94,
        INSTRUMENT_VIEW_5 = 95,
        INSTRUMENT_VIEW_6 = 96,
        INSTRUMENT_VIEW_7 = 97,
        INSTRUMENT_VIEW_8 = 98,
        INSTRUMENT_VIEW_9 = 99,
        LOAD_CUSTOM_CAMERA_0 = 100,
        LOAD_CUSTOM_CAMERA_1 = 101,
        LOAD_CUSTOM_CAMERA_2 = 102,
        LOAD_CUSTOM_CAMERA_3 = 103,
        LOAD_CUSTOM_CAMERA_4 = 104,
        LOAD_CUSTOM_CAMERA_5 = 105,
        LOAD_CUSTOM_CAMERA_6 = 106,
        LOAD_CUSTOM_CAMERA_7 = 107,
        LOAD_CUSTOM_CAMERA_8 = 108,
        LOAD_CUSTOM_CAMERA_9 = 109,
        EXTERNAL_QUICKVIEW_TOP = 110,
        EXTERNAL_QUICKVIEW_REAR = 111,
        EXTERNAL_QUICKVIEW_LEFT = 112,
        EXTERNAL_QUICKVIEW_RIGHT = 113,
        //TOGGLE_EXTERNAL_VIEW = 114, not functional in MSFS2024
    }

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
        public void SetInSim(ExtendedSimConnect simConnect, uint buttonId) => _controller.QuickClick((VJoyButton) buttonId);
    }

    public static class IVJoyControllerExtensions
    {
        private static readonly CancellationToken token = new CancellationToken();

        public static Task<bool> QuickClick(this IVJoyController controller, VJoyButton button)
        {
            return controller.ClickButtonAsync((uint) button, 250, token);
        }

        public static bool PressButton(this IVJoyController controller, VJoyButton button) => controller.PressButton((uint) button);

        public static bool ReleaseButton(this IVJoyController controller, VJoyButton button) => controller.ReleaseButton((uint) button);
    }
}
