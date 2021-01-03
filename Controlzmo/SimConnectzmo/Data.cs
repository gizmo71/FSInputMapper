using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectzmo
{
    public interface IData
    {
        public abstract Type GetStructType();
    }

    public interface Data<StructType> : IData
        where StructType : struct
    {
        Type IData.GetStructType() { return typeof(StructType); }
    }

    public abstract class DataSender<StructType> : Data<StructType>
        where StructType : struct
    {
        public void Send(SimConnect simConnect, StructType data) => simConnect.SetDataOnSimObject(data);
    }
}
