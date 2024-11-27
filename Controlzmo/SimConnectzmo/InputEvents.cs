using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;

namespace Controlzmo.SimConnectzmo
{
    [Component, RequiredArgsConstructor]
    public partial class InputEvents : IOnAircraftLoaded, IButtonCallback<T16000mHotas>
    {
        private readonly ILogger<InputEvents> log;

        private Dictionary<string, SIMCONNECT_INPUT_EVENT_DESCRIPTOR> all = new();

        public int GetButton() => T16000mHotas.BUTTON_FRONT_LEFT_RED;

        public virtual void OnPress(ExtendedSimConnect simConnect) {
            log.LogCritical($"Listing all input events");
            foreach (var (name, rgData) in all)
                Console.WriteLine($"\t{name}\t#{rgData.Hash}\t{rgData.eType}");
        }

        public void OnAircraftLoaded(ExtendedSimConnect simConnect)
        {
            all.Clear();
            simConnect.EnumerateInputEvents(REQUEST.EnumerateInputEvents);
            log.LogCritical($"Asked for input events 1 => {simConnect.GetLastSentPacketID()}");
        }

        internal void OnRecvEnumerateInputEvents(ExtendedSimConnect sc, SIMCONNECT_RECV_ENUMERATE_INPUT_EVENTS data)
        {
            log.LogCritical($"Got some input events {data.dwRequestID}, entry # {data.dwEntryNumber} total of {data.dwOutOf}");
            for (int i = 0; i < data.dwArraySize; ++i)
            {
                var rgData = (SIMCONNECT_INPUT_EVENT_DESCRIPTOR) data.rgData[i];
                try
                {
                    all.Add(rgData.Name, rgData);
                }
                catch (ArgumentException e)
                {
                    // Whatever, we end up with two lots in rapid succession, let's just not care...
                }
            }
        }
    }
}
