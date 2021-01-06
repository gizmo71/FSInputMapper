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
            OnRecvQuit += Handle_OnRecvQuit;
            OnRecvSimobjectData += Handle_OnRecvSimobjectData;
            OnRecvEvent += Handle_OnRecvEvent;
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

        private void Handle_OnRecvOpen(SimConnect _, SIMCONNECT_RECV_OPEN data)
        {
            RegisterDataStructs();

            foreach (IRequestDataOnOpen request in typeToRequest!.Keys.OfType<IRequestDataOnOpen>())
                RequestDataOnSimObject(request, request.GetInitialRequestPeriod());
        }

        private void Handle_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            throw new NotImplementedException("Is it enough to tust through this? Is there anything else we should clean up?");
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
                        dataField.Type, dataField.Epsilon, SIMCONNECT_UNUSED);
                }
                GetType().GetMethod("RegisterDataDefineStruct")!.MakeGenericMethod(type2Struct.Key)
                    .Invoke(this, new object[] { type2Struct.Value });
            }
        }

        public void RequestDataOnSimObject(IDataListener data, SIMCONNECT_PERIOD period)
        {
            REQUEST request = typeToRequest![data];
            STRUCT structId = typeToStruct![data.GetStructType()];
            SIMCONNECT_DATA_REQUEST_FLAG flag = period == SIMCONNECT_PERIOD.ONCE
                            ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                            : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
            RequestDataOnSimObject(request, structId, SIMCONNECT_OBJECT_ID_USER, period, flag, 0, 0, 0);
        }

        private void Handle_OnRecvSimobjectData(SimConnect _, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            REQUEST request = (REQUEST)data.dwRequestID;
            typeToRequest!
                .Where(candidate => candidate.Value == request)
                .Select(candidate => candidate.Key)
                .Single()
                .Process(this, data.dwData[0]);
        }

        private void Handle_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
#if false
            EVENT e = (EVENT)data.uEventID;
//debugConsole.Text = $"Received {e} = {Convert.ToString(data.dwData, 16)} {(int)data.dwData}s (of {data.dwSize})"
//    + $"\n@{System.DateTime.Now}\nGroup ID {(GROUP)data.uGroupID} with ID {data.dwID} and version {data.dwVersion}";
            foreach (KeyValuePair<IEventNotification, EVENT> entry in notificationsToEvent!
                .Where<KeyValuePair<IEventNotification, EVENT>>(candidate => e == candidate.Value))
            {
                entry.Key.OnRecieve(simConnect, data);
            }
#endif
        }
    }
}
