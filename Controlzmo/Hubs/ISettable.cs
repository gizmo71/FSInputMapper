using System;
using SimConnectzmo;

namespace Controlzmo.Hubs
{
    public interface ISettable
    {
        string GetId();
        Type GetValueType();
        void SetInSim(ExtendedSimConnect simConnect, object? value);
    }

    public interface ISettable<ValueType> : ISettable
    {
        Type ISettable.GetValueType() => typeof(ValueType);
        void ISettable.SetInSim(ExtendedSimConnect simConnect, object? value) => SetInSim(simConnect, (ValueType?)value);
        void SetInSim(ExtendedSimConnect simConnect, ValueType? value);

    }
}
