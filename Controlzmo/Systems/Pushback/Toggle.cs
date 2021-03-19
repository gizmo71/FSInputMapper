using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Pushback
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PushbackStateData
    {
        [SimVar("PUSHBACK STATE", null, SIMCONNECT_DATATYPE.STRING32, 0.5f)]
        public string pushbackState;
    }

    [Component]
    public class PushbackToggleListener : DataListener<PushbackStateData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public PushbackToggleListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public override void Process(ExtendedSimConnect simConnect, PushbackStateData data)
        {
            System.Console.Error.WriteLine($"Pushback: state={data.pushbackState}");
        }
    }

    [Component]
    public class TugToggleEvent : IEvent
    {
        public string SimEvent() => "TOGGLE_PUSHBACK";
    }

    [Component]
    public class TugToggleEventListener : IEventNotification, ISettable<object>
    {
        private readonly IEvent tugToggleEvent;

        public TugToggleEventListener(TugToggleEvent tugToggleEvent)
        {
            this.tugToggleEvent = tugToggleEvent;
        }

        public IEvent GetEvent() => tugToggleEvent; // From game

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            SetInSim(simConnect, null);
        }

        public string GetId() => "tugToggle"; // From our UI

        public void SetInSim(ExtendedSimConnect simConnect, object? _)
        {
            //TODO: request state data and issue either the same event if >0 or the disable one if 0
            simConnect.SendEvent(tugToggleEvent);
        }
    }

    [Component]
    public class TugDisableEvent : IEvent
    {
        public string SimEvent() => "TUG_DISABLE";
    }
}
