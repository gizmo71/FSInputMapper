using System;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DataAttribute : Attribute
    {
        public readonly Type DataType;
        public DataAttribute(Type DataType) { this.DataType = DataType; }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DataField : Attribute
    {
        public readonly string Variable;
        public readonly string Units;
        public readonly SIMCONNECT_DATATYPE Type;
        public readonly float Epsilon;
        public DataField(string variable, string units, SIMCONNECT_DATATYPE type, float epsilon)
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
        public readonly DATA Data;
        public readonly SIMCONNECT_PERIOD Period;
        public readonly SIMCONNECT_DATA_REQUEST_FLAG Flag;
        public RequestAttribute(DATA data, SIMCONNECT_PERIOD period)
        {
            Data = data;
            Period = period;
            Flag = Period == SIMCONNECT_PERIOD.ONCE
                ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
        }
    }

}
