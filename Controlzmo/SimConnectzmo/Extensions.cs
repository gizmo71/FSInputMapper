using System;
using System.Linq;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    public static class AttributeExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name!)!.GetCustomAttributes(false).OfType<TAttribute>().Single();
        }
    }

    public static class SimConnectExtensions
    {
        public static void SendEvent(this ExtendedSimConnect sc, IEvent eventToSend, uint data = 0u, bool slow = false, bool fast = false)
        {
            EVENT @event = sc!.eventToEnum![eventToSend];
            SIMCONNECT_EVENT_FLAG flags = 0;
            GROUP? group = sc!.notificationsToEvent!
                .Where(candidate => candidate.Value == @event)
                .Select(_ => GROUP.JUST_MASKABLE)
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
            sc.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, @event, data, group, flags);
        }

        public static void RequestDataOnSimObject(this ExtendedSimConnect simConnect, IDataListener data, SIMCONNECT_PERIOD period)
        {
            REQUEST request = simConnect.typeToRequest![data];
            STRUCT structId = simConnect.typeToStruct![data.GetStructType()];
            SIMCONNECT_DATA_REQUEST_FLAG flag = period == SIMCONNECT_PERIOD.ONCE
                            ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                            : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
            simConnect.RequestDataOnSimObject(request, structId,
                SimConnect.SIMCONNECT_OBJECT_ID_USER, period, flag, 0, 0, 0);
        }

        public static void SetDataOnSimObject<StructType>(this SimConnect simConnect, StructType data)
            where StructType : struct
        {
            STRUCT id = (simConnect as ExtendedSimConnect)!.typeToStruct![typeof(StructType)];
            simConnect.SetDataOnSimObject(id, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, data);
        }
    }
}
