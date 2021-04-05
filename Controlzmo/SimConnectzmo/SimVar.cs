using System;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class ClientVarAttribute : Attribute
    {
        public readonly float Epsilon;

        public ClientVarAttribute(float epsilon)
        {
            Epsilon = epsilon;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class SimVarAttribute : ClientVarAttribute
    {
        public readonly string Variable;
        public readonly string? Units;
        public readonly SIMCONNECT_DATATYPE Type;

        public SimVarAttribute(string variable, string? units, SIMCONNECT_DATATYPE type, float epsilon) : base(epsilon)
        {
            Variable = variable;
            Units = units;
            Type = type; //TODO: do we need this, or can we infer it?
        }
    }
}
