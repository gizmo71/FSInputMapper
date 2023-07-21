using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;

//45322 by 1103 16 buttons T16000M stick
//45845 by 1103 12 buttons Ferrari gamepad
namespace Controlzmo.GameControllers
{
    [Component]
    public class GameControllers : CreateOnStartup
    {
        private readonly ILogger _log;
        private readonly IEnumerable<GameController> controllers;

        public GameControllers(IServiceProvider sp)
        {
            _log = sp.GetRequiredService<ILogger<GameControllers>>();
            controllers = sp.GetServices<GameController>();
            RawGameController.RawGameControllerAdded += Added;
            RawGameController.RawGameControllerRemoved += Removed;
            RawGameController.RawGameControllers.ToList().ForEach(c => Added(null, c));
        }

        private void Added(object? sender, RawGameController c)
        {
            _log.LogDebug($"Controller {c.HardwareVendorId} {c.HardwareProductId} = {c.DisplayName} added");
            foreach (var gc in controllers)
                gc.Offer(c);
        }

        private void Removed(object? sender, RawGameController c)
        {
            // Is there actually any reason to do anything?
        }
    }

    public abstract class GameController : IOnSimFrame
    {
        public abstract ushort Vendor();
        public abstract ushort Product();

        private readonly bool[] buttonState;
        private ulong? lastReadingTimestamp;
        protected readonly ILogger _log;
        private RawGameController? raw;

        protected GameController(IServiceProvider sp, int buttons)
        {
            buttonState = new bool[buttons];
            _log = sp.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName!);
        }

        public void Offer(RawGameController candidate)
        {
            if (Vendor() != candidate.HardwareVendorId || Product() != candidate.HardwareProductId) {
                return;
            }
            raw = candidate;
            _log.LogDebug($"{GetHashCode()} claimed {raw} {buttonState.Length} = {raw.ButtonCount}?");
        }

        private double[] axisArray = { };
        private GameControllerSwitchPosition[] switchArray = { };
        public void OnFrame(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT_FRAME data)
        {
//_log.LogCritical($"{raw} in {GetHashCode()}");
            var buttonArray = new bool[buttonState.Length]; //TODO: just create this once when the class starts.
            var timestamp = raw?.GetCurrentReading(buttonArray, switchArray, axisArray);
//_log.LogCritical($"{lastReadingTimestamp} -> {timestamp} @ {raw}");
            if (timestamp == lastReadingTimestamp) return;
            lastReadingTimestamp = timestamp;
            for (int i = 0; i < buttonState.Length; ++i)
                if (buttonArray[i] != buttonState[i])
                    OnButtonChange(simConnect, i, buttonState[i] = buttonArray[i]);
        }

        public abstract void OnButtonChange(ExtendedSimConnect simConnect, int index, bool isPressed);
    }
}