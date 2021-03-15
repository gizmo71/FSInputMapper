using System;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class SimVarAttribute : Attribute
    {
        public readonly string Variable;
        public readonly string? Units;
        public readonly SIMCONNECT_DATATYPE Type;
        public readonly float Epsilon;

        public SimVarAttribute(string variable, string? units, SIMCONNECT_DATATYPE type, float epsilon)
        {
            Variable = variable;
            Units = units;
            Type = type; //TODO: do we need this, or can we infer it?
            Epsilon = epsilon;
        }
    }
}
