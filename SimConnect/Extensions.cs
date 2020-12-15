using System;
using System.Linq;
using FSInputMapper.Data;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    public static class SimConnectExtensions
    {
        public static void SendEvent(this SimConnect sc, IEvent eventToSend, uint data = 0u, bool slow = false, bool fast = false)
        {
            EVENT e = (sc as SimConnectzmo)!.eventToEnum![eventToSend];
            sc.SendEvent(e, data, slow, fast);
        }
//TODO: remove all callers of the below and merge it with the above...
        public static void SendEvent(this SimConnect sc, EVENT eventToSend, uint data = 0u, bool slow = false, bool fast = false)
        {
            SIMCONNECT_EVENT_FLAG flags = 0;
            GROUP? group = (sc as SimConnectzmo)!.notificationsToEvent!
                .Where(candidate => candidate.Value == eventToSend)
                .Select(notification => notification.Key.GetGroup())
                .Distinct()
                .Cast<GROUP?>()
                .DefaultIfEmpty(null)
                .Single();
            if (group == null)
            {
                group = (GROUP)SimConnect.SIMCONNECT_GROUP_PRIORITY_STANDARD;
                flags |= SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY;
            }
//MessageBox.Show($"event {eventToSend} group " + (group != null ? group.ToString() : "none") + $" data {data}", "SendEvent", MessageBoxButton.OK, MessageBoxImage.Warning);
            if (slow) flags |= SIMCONNECT_EVENT_FLAG.SLOW_REPEAT_TIMER;
            if (fast) flags |= SIMCONNECT_EVENT_FLAG.FAST_REPEAT_TIMER;
            sc.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, data, group, flags);
        }

        public static void RequestDataOnSimObject(this SimConnect simConnect, IDataListener data, SIMCONNECT_PERIOD period)
        {
            if (simConnect is SimConnectzmo sc)
            {
                REQUEST request = sc.typeToRequest![data];
                STRUCT structId = sc.typeToStruct![data.GetStructType()];
                SIMCONNECT_DATA_REQUEST_FLAG flag = period == SIMCONNECT_PERIOD.ONCE
                                ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                                : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
                sc.RequestDataOnSimObject(request, structId,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER, period, flag, 0, 0, 0);
            }
            else
                throw new NullReferenceException("Supplied SimConnect was not correct subclass");

        }

        public static void SetDataOnSimObject<StructType>(this SimConnect simConnect, StructType data)
            where StructType : struct
        {
            if (simConnect is SimConnectzmo sc)
            {
                STRUCT id = sc.typeToStruct![typeof(StructType)];
                sc.SetDataOnSimObject(id, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, data);
            }
        }

    }

}
