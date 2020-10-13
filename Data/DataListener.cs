using System;

namespace FSInputMapper.Data
{

    public abstract class IDataListener
    {
        public abstract Type GetStructType();
        public abstract void Process(object data);
    }

    public abstract class DataListener<StructType> : IDataListener
    {
        public override Type GetStructType() { return typeof(StructType); }
        public override void Process(object data) { Process((StructType)data); }
        public abstract void Process(StructType data);
    }

}
