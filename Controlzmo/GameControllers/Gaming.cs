using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
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

    public interface IGameController
    {
        public void Offer(RawGameController candidate);
    }

    [Component]
    public abstract class GameController<T> : IGameController, IOnSimFrame where T : GameController<T>
    {
        public abstract ushort Vendor();
        public abstract ushort Product();

        protected readonly bool[] buttonsOld;
        protected readonly bool[] buttonsNew;
        protected readonly double[] axesOld;
        protected readonly double[] axesNew;
        protected readonly GameControllerSwitchPosition[] switchesOld;
        protected readonly GameControllerSwitchPosition[] switchesNew;

        private ulong? lastReadingTimestamp;
        protected readonly ILogger _log;
        private readonly IEnumerable<IButtonCallback<T>> buttonCallbacks;
        private RawGameController? raw;

        protected GameController(IServiceProvider sp, int buttons, int axes, int switches)
        {
            buttonsOld = new bool[buttons];
            buttonsNew = (bool[])buttonsOld.Clone();
            axesOld = new double[axes];
            axesNew = (double[])axesOld.Clone();
            switchesOld = new GameControllerSwitchPosition[switches]; //TODO: initialise to centre?
            switchesNew = (GameControllerSwitchPosition[]) switchesOld.Clone();
            _log = sp.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName!);
            buttonCallbacks = sp.GetServices<IButtonCallback<T>>();
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
            OnUpdate(simConnect);

            foreach (var callback in buttonCallbacks)
            {
                if (!buttonsOld[callback.GetButton()] && buttonsNew[callback.GetButton()])
                    callback.OnPress(simConnect);
                else if (buttonsOld[callback.GetButton()] && !buttonsNew[callback.GetButton()])
                    callback.OnRelease(simConnect);
            }

            buttonsNew.CopyTo(buttonsOld, 0);
            axesNew.CopyTo(axesOld, 0);
            switchesNew.CopyTo(switchesOld, 0);
        }

        protected abstract void OnUpdate(ExtendedSimConnect simConnect);
    }
}