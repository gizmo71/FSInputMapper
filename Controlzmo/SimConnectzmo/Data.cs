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
        public abstract void Process(ExtendedSimConnect simConnect, object data);
    }

    public interface IData<StructType> : IData
        where StructType : struct
    {
        Type IData.GetStructType() { return typeof(StructType); }
    }

    public abstract class DataListener<StructType> : IData<StructType>, IDataListener
        where StructType : struct
    {
        public void Process(ExtendedSimConnect simConnect, object data)
        {
            Process(simConnect, (StructType)data);
        }
        public abstract void Process(ExtendedSimConnect simConnect, StructType data);
    }

    public interface IRequestDataOnOpen : IDataListener
    {
        public abstract SIMCONNECT_PERIOD GetInitialRequestPeriod();
    }

    public abstract class DataSender<StructType> : IData<StructType>
        where StructType : struct
    {
        public void Send(ExtendedSimConnect? simConnect, StructType data) => simConnect.SendDataOnSimObject(data);
    }
}
