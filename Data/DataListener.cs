using System;

namespace FSInputMapper.Data
{

    public abstract class IDataListener
    {
        public abstract Type GetStructType();
        public abstract void Process(SimConnectAdapter adapter, object data);
//TODO: get rid of this and drive REQUEST values from DataListeners instead
        public abstract bool Accept(REQUEST request);
    }

    public abstract class DataListener<StructType> : IDataListener
    {
        public override Type GetStructType() { return typeof(StructType); }
        public override void Process(SimConnectAdapter adapter, object data) { Process(adapter, (StructType)data); }
        public abstract void Process(SimConnectAdapter adapter, StructType data);
        public override bool Accept(REQUEST request) { return true; }
    }

}
