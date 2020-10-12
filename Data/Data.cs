using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text;

//TODO: Use these as pure listeners?
namespace FSInputMapper.Data
{

    public abstract class IData
    {
        public abstract Type GetStructType();
        public abstract void Process(object data);
    }

    public abstract class Data<StructType> : IData
    {
        public override Type GetStructType() { return typeof(StructType); }
        public override void Process(object data) { Process((StructType)data); }
        public abstract void Process(StructType data);
    }

}
