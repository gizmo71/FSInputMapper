using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;

//45322 by 1103, T16000M stick: 16 buttons, 4 axes, 1 switch
//45845 by 1103, Ferrari gamepad: 12 buttons, 4 axes, 1 switch
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
            _log.LogDebug($"\thas {c.ButtonCount} buttons, {c.AxisCount} axes, {c.SwitchCount} switches");
            foreach (var gc in controllers)
                gc.Offer(c);
        }

        private void Removed(object? sender, RawGameController c)
        {
            // Is there actually any reason to do anything?
        }
    }

    [Component]
    public abstract class GameController : IOnSimFrame
    {
        public abstract ushort Vendor();
        public abstract ushort Product();

        private readonly bool[] buttonsOld;
        private readonly double[] axesOld;
        private readonly GameControllerSwitchPosition[] switchesOld;
        private readonly bool[] buttonsNew;
        private readonly double[] axesNew;
        private readonly GameControllerSwitchPosition[] switchesNew;
        private ulong? lastReadingTimestamp;
        protected readonly ILogger _log;
        private RawGameController? raw;

        protected GameController(IServiceProvider sp, int buttons, int axes, int switches)
        {
            buttonsOld = new bool[buttons];
            buttonsNew = (bool[])buttonsOld.Clone();
            axesOld = new double[axes];
            axesNew = (double[])axesOld.Clone();
            switchesOld = new GameControllerSwitchPosition[switches];
            switchesNew = (GameControllerSwitchPosition[]) switchesOld.Clone();
            _log = sp.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName!);
        }

        public void Offer(RawGameController candidate)
        {
            if (Vendor() != candidate.HardwareVendorId || Product() != candidate.HardwareProductId) {
                return;
            }
            raw = candidate;
            _log.LogDebug($"{GetHashCode()} claimed {raw} {buttonsOld.Length} = {raw.ButtonCount}?");
        }

        public void OnFrame(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT_FRAME data)
        {
//_log.LogCritical($"{raw} in {GetHashCode()}");
            var timestamp = raw?.GetCurrentReading(buttonsNew, switchesNew, axesNew);
//_log.LogCritical($"{lastReadingTimestamp} -> {timestamp} @ {raw}");
            if (timestamp == lastReadingTimestamp) return;
            lastReadingTimestamp = timestamp;
            for (int i = 0; i < buttonsOld.Length; ++i)
                if (buttonsNew[i] != buttonsOld[i])
                    OnButtonChange(simConnect, i, buttonsOld[i] = buttonsNew[i]);
        }

        public abstract void OnButtonChange(ExtendedSimConnect simConnect, int index, bool isPressed);
    }
}