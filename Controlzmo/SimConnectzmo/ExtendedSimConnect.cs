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
    internal enum GROUP { JUST_MASKABLE = 666 }

    public class ExtendedSimConnect : SimConnect
    {
        private static readonly IntPtr hWnd = IntPtr.Zero;

        internal Dictionary<Type, STRUCT>? typeToStruct;
        internal Dictionary<IDataListener, REQUEST>? typeToRequest;
        internal Dictionary<IEvent, EVENT>? eventToEnum;
        internal Dictionary<IEventNotification, EVENT>? notificationsToEvent;

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

            eventToEnum = serviceProvider.GetServices<IEvent>()
                .Select((e, index) => new ValueTuple<IEvent, EVENT>(e, (EVENT)(index + 1)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            notificationsToEvent = serviceProvider.GetServices<IEventNotification>()
                .ToDictionary(en => en, en => eventToEnum[en.GetEvent()]);

            return this;
        }

        private void Handle_OnRecvOpen(SimConnect _, SIMCONNECT_RECV_OPEN data)
        {
            RegisterDataStructs();
            MapClientEvents();
            SetGroupPriorities();

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

        private void MapClientEvents()
        {
            foreach (var e in Enum.GetValues(typeof(EVENT)).OfType<EVENT>())
                MapClientEventToSimEvent(e, e.GetAttribute<EventAttribute>().ClientEvent);
            foreach (var eventToEnum in eventToEnum!)
                MapClientEventToSimEvent(eventToEnum.Value, eventToEnum.Key.SimEvent());
            foreach (var notificationToEvent in notificationsToEvent!)
                AddClientEventToNotificationGroup(GROUP.JUST_MASKABLE, notificationToEvent.Value, true);
        }

        private void SetGroupPriorities()
        {
            SetNotificationGroupPriority(GROUP.JUST_MASKABLE, SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE);
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

        private void Handle_OnRecvEvent(SimConnect _, SIMCONNECT_RECV_EVENT data)
        {
            EVENT e = (EVENT)data.uEventID;
//debugConsole.Text = $"Received {e} = {Convert.ToString(data.dwData, 16)} {(int)data.dwData}s (of {data.dwSize})"
//    + $"\n@{System.DateTime.Now}\nGroup ID {(GROUP)data.uGroupID} with ID {data.dwID} and version {data.dwVersion}";
            foreach (IEventNotification notification in notificationsToEvent!
                .Where(candidate => e == candidate.Value)
                .Select(candidate => candidate.Key))
            {
                notification.OnRecieve(this, data);
            }
        }
    }
}
