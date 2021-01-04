using System;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    public interface IData
    {
        public abstract Type GetStructType();
    }

    public interface IDataListener : IData
    {
        public abstract void Process(SimConnect simConnect, object data);
    }

    public interface Data<StructType> : IData
        where StructType : struct
    {
        Type IData.GetStructType() { return typeof(StructType); }
    }

    public abstract class DataListener<StructType> : Data<StructType>, IDataListener
        where StructType : struct
    {
        public void Process(SimConnect simConnect, object data) { Process(simConnect, (StructType)data); }
        public abstract void Process(SimConnect simConnect, StructType data);
    }

    public interface IRequestDataOnOpen : IDataListener
    {
        public abstract SIMCONNECT_PERIOD GetInitialRequestPeriod();
    }

    public abstract class DataSender<StructType> : Data<StructType>
        where StructType : struct
    {
        public void Send(SimConnect simConnect, StructType data) => simConnect.SetDataOnSimObject(data);
    }
}
