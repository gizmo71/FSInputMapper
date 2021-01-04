using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.FlightSimulator.SimConnect;
using Microsoft.Extensions.DependencyInjection;

namespace SimConnectzmo
{
    internal enum REQUEST { }
    internal enum STRUCT { }
    internal enum EVENT { }

    public class ExtendedSimConnect : SimConnect
    {
        private static readonly IntPtr hWnd = IntPtr.Zero;

        internal Dictionary<Type, STRUCT>? typeToStruct;
        internal Dictionary<IDataListener, REQUEST>? typeToRequest;
        //internal Dictionary<IEventNotification, EVENT>? notificationsToEvent;
        //internal Dictionary<IEvent, EVENT>? eventToEnum;

        internal ExtendedSimConnect(string szName, uint UserEventWin32, WaitHandle waitHandle)
            : base(szName, hWnd, UserEventWin32, waitHandle, 0) // 6 for over IP - can we make it timeout easier?
        {
            OnRecvOpen += Handle_OnRecvOpen;
        }

        internal ExtendedSimConnect AssignIds(IServiceProvider serviceProvider)
        {
            typeToStruct = serviceProvider
                .GetServices<IData>()
                .Select(candidate => candidate.GetStructType())
                .Distinct()
                .Select((structType, index) => new ValueTuple<Type, STRUCT>(structType, (STRUCT)(index + 1)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            typeToRequest = serviceProvider.GetServices<IDataListener>()
                .Select((request, index) => new ValueTuple<IDataListener, REQUEST>(request, (REQUEST)(index + 1)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
            //TODO: check that the struct is also registered

            return this;
        }

        private void Handle_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            RegisterDataStructs();
        }

        private void RegisterDataStructs()
        {
            foreach (var type2Struct in typeToStruct!)
            {
                foreach (FieldInfo field in type2Struct.Key.GetFields())
                {
                    var dataField = field.GetCustomAttribute<SimVarAttribute>();
                    if (dataField == null)
                        throw new NullReferenceException($"No DataField for {type2Struct.Key}.{field.Name}");
                    AddToDataDefinition(type2Struct.Value, dataField.Variable, dataField.Units,
                        dataField.Type, dataField.Epsilon, SimConnect.SIMCONNECT_UNUSED);
                }
                GetType().GetMethod("RegisterDataDefineStruct")!.MakeGenericMethod(type2Struct.Key)
                    .Invoke(this, new object[] { type2Struct.Value });
            }
        }
    }
}
