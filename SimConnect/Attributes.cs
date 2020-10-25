using System;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class SCStructFieldAttribute : Attribute
    {

        public readonly string Variable;
        public readonly string Units;
        public readonly SIMCONNECT_DATATYPE Type;
        public readonly float Epsilon;

        public SCStructFieldAttribute(string variable, string units, SIMCONNECT_DATATYPE type, float epsilon)
        {
            Variable = variable;
            Units = units;
            Type = type; //TODO: do we need this, or can we infer it?
            Epsilon = epsilon;
        }

    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class EventAttribute : Attribute
    {

        public readonly string ClientEvent;
        public readonly Type? EventNotificationType;

        public EventAttribute(string clientEvent) : this(clientEvent, null) { }

        public EventAttribute(string clientEvent, Type? eventNotificationType)
        {
            ClientEvent = clientEvent;
            EventNotificationType = eventNotificationType;
            if (EventNotificationType != null && !typeof(IEventNotification).IsAssignableFrom(EventNotificationType))
                throw new ArgumentException($"{EventNotificationType!.Name} is not an IEventNotification");
        }

    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public abstract class GroupAttribute : Attribute
    {
        public uint Priority;
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class HighestMaskablePriorityGroupAttribute : GroupAttribute
    {
        public HighestMaskablePriorityGroupAttribute()
        {
            Priority = SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE;
        }
    }

}
