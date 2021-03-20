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
        [SimVar("PUSHBACK STATE", null, SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public uint pushbackState;
    }

    [Component]
    public class PushbackToggleListener : DataListener<PushbackStateData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        bool isPushbackActive;
        public bool IsPushbackActive => isPushbackActive;

        public PushbackToggleListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, PushbackStateData data)
        {
            isPushbackActive = data.pushbackState != 3;
        }
    }

    [Component]
    public class TugDisableEvent : IEvent
    {
        public string SimEvent() => "TUG_DISABLE";
    }

    [Component]
    public class TugToggleEvent : IEvent
    {
        public string SimEvent() => "TOGGLE_PUSHBACK";
    }

    [Component]
    public class TugToggleEventListener : IEventNotification, ISettable<object>
    {
        private readonly TugToggleEvent tugToggleEvent;
        private readonly TugDisableEvent tugDisableEvent;
        private readonly PushbackToggleListener stateListener;

        public TugToggleEventListener(TugToggleEvent tugToggleEvent, TugDisableEvent tugDisableEvent, PushbackToggleListener stateListener)
        {
            this.tugToggleEvent = tugToggleEvent;
            this.tugDisableEvent = tugDisableEvent;
            this.stateListener = stateListener;
        }

        public IEvent GetEvent() => tugToggleEvent; // From game

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            SetInSim(simConnect, null);
        }

        public string GetId() => "tugToggle"; // From our UI

        public void SetInSim(ExtendedSimConnect simConnect, object? _)
        {
            simConnect.SendEvent(stateListener.IsPushbackActive ? tugDisableEvent : tugToggleEvent);
        }
    }
}
