using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using Windows.Gaming.Input;

namespace Controlzmo.GameControllers
{
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
        private readonly IEnumerable<ISwitchCallback<T>> switchCallbacks;
        private readonly IEnumerable<IAxisCallback<T>> axisCallbacks;
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
            switchCallbacks = sp.GetServices<ISwitchCallback<T>>();
            axisCallbacks = sp.GetServices<IAxisCallback<T>>();
        }

        public void Offer(RawGameController candidate)
        {
            if (Vendor() != candidate.HardwareVendorId || Product() != candidate.HardwareProductId) {
                return;
            }
            raw = candidate;
            _log.LogDebug($"{GetHashCode()} claimed {raw} {buttonsOld.Length} = {raw.ButtonCount}?");
            OnConnected();
        }

        protected virtual void OnConnected() { }

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
            foreach (var callback in switchCallbacks)
                if (switchesOld[callback.GetSwitch()] != switchesNew[callback.GetSwitch()])
                    callback.OnChange(simConnect, switchesOld[callback.GetSwitch()], switchesNew[callback.GetSwitch()]);
            foreach (var callback in axisCallbacks)
                if (axesOld[callback.GetAxis()] != axesNew[callback.GetAxis()])
                    callback.OnChange(simConnect, axesOld[callback.GetAxis()], axesNew[callback.GetAxis()]);

            buttonsNew.CopyTo(buttonsOld, 0);
            axesNew.CopyTo(axesOld, 0);
            switchesNew.CopyTo(switchesOld, 0);
        }

        protected abstract void OnUpdate(ExtendedSimConnect simConnect);
    }
}