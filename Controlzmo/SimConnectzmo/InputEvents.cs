using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;

namespace Controlzmo.SimConnectzmo
{
    [Component, RequiredArgsConstructor]
    public partial class InputEvents : IOnAircraftLoaded, ISettable<string?>
    {
        private readonly ILogger<InputEvents> log;

        private Dictionary<string, SIMCONNECT_INPUT_EVENT_DESCRIPTOR> all = new();

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            log.LogCritical($"Listing all input events");
            foreach (var (name, rgData) in all)
                Console.WriteLine($"\t{name}\t#{rgData.Hash}\t{rgData.eType}");
        }

        public string GetId() => "listInputEvents";

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

        internal void Send(ExtendedSimConnect sc, string name, object value)
        {
            SIMCONNECT_INPUT_EVENT_DESCRIPTOR @event;
            if (!all.TryGetValue(name, out @event!))
            {
                log.LogError($"No event with name {name}");
                return;
            }
            log.LogInformation($"Getting value of {name} {@event.Hash} and sending it with {value}");
            sc.SetInputEvent(@event.Hash, value);
            sc.GetInputEvent(REQUEST.InputEventExperimental, @event.Hash);
            sc.EnumerateInputEventParams(@event.Hash);
            //TODO: can we record whether we've asked and only do it once per input event?
        }

        internal void OnRecvGetInputEvent(ExtendedSimConnect sc, SIMCONNECT_RECV_GET_INPUT_EVENT data)
        {
            // Perhaps some event somewhere actually has a useful value associated with it...
            Console.WriteLine($"GetInputEvent returned {data.Value[0]} of type {data.eType} for {data.dwID}");
        }

        internal void OnRecvEnumerateInputEventParams(ExtendedSimConnect sc, SIMCONNECT_RECV_ENUMERATE_INPUT_EVENT_PARAMS data)
        {
            Console.WriteLine($"Params for {data.Hash} enumerated as {data.Value}");
        }
    }
}
