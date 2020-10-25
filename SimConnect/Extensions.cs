using System;
using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    public static class SimConnectExtensions
    {

        public static void SendEvent(this SimConnect sc, EVENT eventToSend, uint data = 0u, bool slow = false, bool fast = false)
        {
            SIMCONNECT_EVENT_FLAG flags = 0;
            GROUP group = (GROUP)SimConnect.SIMCONNECT_GROUP_PRIORITY_STANDARD;
            var groupAttribute = eventToSend!.GetAttribute<EventGroupAttribute>();
            if (groupAttribute != null)
                group = groupAttribute.Group;
            else
                flags |= SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY;
            if (slow) flags |= SIMCONNECT_EVENT_FLAG.SLOW_REPEAT_TIMER;
            if (fast) flags |= SIMCONNECT_EVENT_FLAG.FAST_REPEAT_TIMER;
            sc.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, data, group, flags);
        }

        public static void RequestDataOnSimObject(this SimConnect simConnect, IDataListener data, SIMCONNECT_PERIOD period)
        {
            var sc = simConnect as SimConnectzmo;
            if (sc == null) throw new NullReferenceException("Supplied SimConnect was not correct subclass");
            REQUEST request = sc.typeToRequest![data];
            STRUCT structId = sc.typeToStruct![data.GetStructType()];
            SIMCONNECT_DATA_REQUEST_FLAG flag = period == SIMCONNECT_PERIOD.ONCE
                            ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                            : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
            sc.RequestDataOnSimObject(request, structId,
                SimConnect.SIMCONNECT_OBJECT_ID_USER, period, flag, 0, 0, 0);
        }

    }

}
