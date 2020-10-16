using System;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    public abstract class IDataListener
    {
        public abstract Type GetStructType();
        public virtual SIMCONNECT_PERIOD GetPeriod() { return SIMCONNECT_PERIOD.ONCE; }
        public SIMCONNECT_DATA_REQUEST_FLAG GetFlag()
        {
            return GetPeriod() == SIMCONNECT_PERIOD.ONCE
                ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
        }
        public abstract void Process(SimConnectAdapter adapter, object data);
    }

    public abstract class DataListener<StructType> : IDataListener
    {
        public override Type GetStructType() { return typeof(StructType); }
        public override void Process(SimConnectAdapter adapter, object data) { Process(adapter, (StructType)data); }
        public abstract void Process(SimConnectAdapter adapter, StructType data);
    }

}
