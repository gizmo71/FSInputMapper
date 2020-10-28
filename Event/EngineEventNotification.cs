using System;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Event
{

    [Singleton]
    public class ThrottleSetEventNotification : IEventNotification
    {

        public GROUP GetGroup()
        {
            return GROUP.ENGINE;
        }

        public void OnRecieve(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            simConnect.SendEvent((EVENT)data.uEventID, data.dwData);
            double axis = ((int)data.dwData + 16384) / 327.68;
        }

    }

}