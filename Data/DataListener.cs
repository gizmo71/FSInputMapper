using System;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    public interface IData
    {
        public abstract Type GetStructType();
    }

    public interface IDataListener : IData
    {
        public abstract void Process(SimConnectAdapter adapter, object data);
    }

    public abstract class Data<StructType> : IData
    {
        public Type GetStructType() { return typeof(StructType); }
    }

    public abstract class DataSender<StructType> : Data<StructType> { }

    public abstract class DataListener<StructType> : Data<StructType>, IDataListener
    {
        public void Process(SimConnectAdapter adapter, object data) { Process(adapter, (StructType)data); }
        public abstract void Process(SimConnectAdapter adapter, StructType data);
    }

}
