using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
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

    //[Component]
    public class TugDisableEvent : IEvent
    {
        public string SimEvent() => "TUG_DISABLE";
    }

    //[Component]
    public class PushbackStateListener : DataListener<PushbackStateData>, IRequestDataOnOpen
    {
        private readonly TugHeadingListener headingListener;
        private readonly TugDisableEvent tugDisableEvent;

        public PushbackStateListener(IServiceProvider serviceProvider)
        {
            this.headingListener = serviceProvider.GetRequiredService<TugHeadingListener>();
            this.tugDisableEvent = serviceProvider.GetRequiredService<TugDisableEvent>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, PushbackStateData data)
        {
            SIMCONNECT_PERIOD listenerPeriod;
            if (data.pushbackState != 3)
                listenerPeriod = SIMCONNECT_PERIOD.VISUAL_FRAME;
            else
            {
                simConnect.SendEvent(tugDisableEvent);
                listenerPeriod = SIMCONNECT_PERIOD.NEVER;
            }
            simConnect.RequestDataOnSimObject(headingListener, listenerPeriod);
        }
    }
}
