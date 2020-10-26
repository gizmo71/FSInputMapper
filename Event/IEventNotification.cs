using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Event
{

    [ProvideDerived]
    public interface IEventNotification
    {
        public abstract GROUP GetGroup();
        public abstract void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data);
    }

}
