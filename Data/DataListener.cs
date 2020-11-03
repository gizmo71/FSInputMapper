using System;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [ProvideDerived]
    public interface IData
    {
        public abstract Type GetStructType();
    }

    [ProvideDerived]
    public interface IDataListener : IData
    {
        public abstract void Process(SimConnect simConnect, object data);
    }

    public interface IRequestDataOnOpen : IDataListener
    {
        public abstract SIMCONNECT_PERIOD GetInitialRequestPeriod();
    }

    public abstract class Data<StructType> : IData
    {
        public Type GetStructType() { return typeof(StructType); }
    }

    public abstract class DataListener<StructType> : Data<StructType>, IDataListener
    {
        public void Process(SimConnect simConnect, object data) { Process(simConnect, (StructType)data); }
        public abstract void Process(SimConnect simConnect, StructType data);
    }

}
