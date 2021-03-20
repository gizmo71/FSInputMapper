using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
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
    public class PushbackState
    {
        public bool IsPushbackActive { get; set; }
    }

    [Component]
    public class PushbackStateListener : DataListener<PushbackStateData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly PushbackState state;
        private readonly TugHeadingListener headingListener;

        public PushbackStateListener(IServiceProvider serviceProvider)
        {
            this.hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            this.state = serviceProvider.GetRequiredService<PushbackState>();
            this.headingListener = serviceProvider.GetRequiredService<TugHeadingListener>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, PushbackStateData data)
        {
            state.IsPushbackActive = data.pushbackState != 3;
            var period = state.IsPushbackActive ? SIMCONNECT_PERIOD.VISUAL_FRAME : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(headingListener, period);
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
    public class TugToggleEventListener : IEventNotification
    {
        private readonly TugToggleEvent tugToggleEvent;
        private readonly TugDisableEvent tugDisableEvent;
        private readonly PushbackState state;

        public TugToggleEventListener(IServiceProvider serviceProvider)
        {
            this.tugToggleEvent = serviceProvider.GetRequiredService<TugToggleEvent>();
            this.tugDisableEvent = serviceProvider.GetRequiredService<TugDisableEvent>();
            this.state = serviceProvider.GetRequiredService<PushbackState>();
        }

        public IEvent GetEvent() => tugToggleEvent;

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            simConnect.SendEvent(state.IsPushbackActive ? tugDisableEvent : tugToggleEvent);
        }
    }
}
