﻿using System;
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
    public class RequestAttribute : Attribute
    {
        public readonly Type DataType;
        public readonly SIMCONNECT_PERIOD Period;
        public readonly SIMCONNECT_DATA_REQUEST_FLAG Flag;
        public RequestAttribute(Type dataType, SIMCONNECT_PERIOD period)
        {
            DataType = dataType;
            Period = period;
            Flag = Period == SIMCONNECT_PERIOD.ONCE
                ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class EventAttribute : Attribute
    {
        public readonly string ClientEvent;
        public EventAttribute(string clientEvent) { ClientEvent = clientEvent; }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class EventGroupAttribute : Attribute
    {
        public readonly GROUP Group;
        public readonly bool IsMaskable;
        public EventGroupAttribute(GROUP group, bool isMaskable)
        {
            Group = group;
            IsMaskable = isMaskable;
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

    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class SCStructAttribute : Attribute
    {
    }

}