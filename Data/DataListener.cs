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

    public abstract class DataSender<StructType> : Data<StructType>
    {

        private readonly SimConnectHolder simConnectHolder;

        protected DataSender(SimConnectHolder simConnectHolder)
        {
            this.simConnectHolder = simConnectHolder;
        }

        public void Send(StructType value)
        {
            if (simConnectHolder.SimConnect is SimConnectzmo sc)
            {
                STRUCT id = sc.typeToStruct![value!.GetType()];
                sc.SetDataOnSimObject(id, SimConnect.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_DATA_SET_FLAG.DEFAULT, value);
            }
        }

    }

    public abstract class DataListener<StructType> : Data<StructType>, IDataListener
    {
        public void Process(SimConnectAdapter adapter, object data) { Process(adapter, (StructType)data); }
        public abstract void Process(SimConnectAdapter adapter, StructType data);
    }

}
