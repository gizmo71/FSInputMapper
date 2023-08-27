﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using Controlzmo;
using System.Text.RegularExpressions;
using Windows.Networking.Sockets;
using Microsoft.AspNetCore.Http.Headers;

namespace SimConnectzmo
{
    // Change ENUM_DYNAMIC_START if you need more internal values.
    internal enum REQUEST { AircraftLoaded = 1, SimSystemState }
    internal enum STRUCT { }
    internal enum EVENT { SimSystemState = 1, Frame }
    internal enum GROUP { JUST_MASKABLE = 1 }

    public class ExtendedSimConnect : SimConnect
    {
        private static readonly IntPtr hWnd = IntPtr.Zero;

        private ILogger<ExtendedSimConnect>? _logging;
        private SerializedExecutor? serializedExecutor;

        private Dictionary<Type, STRUCT>? typeToStruct;
        private Dictionary<Type, string>? typeToClientDataName;
        private Dictionary<IDataListener, REQUEST>? typeToRequest;
        private Dictionary<IEvent, EVENT>? eventToEnum;
        private Dictionary<IEventNotification, EVENT>? notificationsToEvent;
        private IEnumerable<IOnSimConnection>? onConnectionHandlers;
        private IEnumerable<IOnSimStarted>? onSimStartedHandlers;
        private IEnumerable<IOnSimFrame>? onSimFrameHandlers;

        public bool? IsSimStarted;

        internal ExtendedSimConnect(string szName, uint UserEventWin32, WaitHandle waitHandle)
            : base(szName, hWnd, UserEventWin32, waitHandle, 0) // 6 for over IP - can we make it timeout easier?
        {
            OnRecvOpen += Handle_OnRecvOpen;
            OnRecvQuit += Handle_OnRecvQuit;
            OnRecvSimobjectData += Handle_OnRecvSimobjectData;
            OnRecvClientData += Handle_OnRecvSimobjectData;
            OnRecvEvent += Handle_OnRecvEvent;
            OnRecvEventFrame += Handle_OnRecvEventFrame;
            OnRecvException += Handle_Exception;
            OnRecvSystemState += Handle_OnRecvSystemState;

//TODO: What's this for?
            //FieldInfo? fiSimConnect = typeof(SimConnect).GetField("hSimConnect", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void Handle_Exception(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            _logging?.LogError($"Got exception {data.dwException} packet {data.dwSendID}");
        }

        internal ExtendedSimConnect AssignIds(IServiceProvider serviceProvider)
        {
            const int ENUM_DYNAMIC_START = 10; // Leave some space for explicit enum values.
            _logging = serviceProvider.GetRequiredService<ILogger<ExtendedSimConnect>>();
            serializedExecutor = serviceProvider.GetRequiredService<SerializedExecutor>();

            typeToStruct = serviceProvider
                .GetServices<IData>()
                .Select(candidate => candidate.GetStructType())
                .Distinct()
                .Select((structType, index) => new ValueTuple<Type, STRUCT>(structType, (STRUCT)(index + ENUM_DYNAMIC_START)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            typeToClientDataName = serviceProvider
                .GetServices<IClientData>()
                .Select((clientData, index) => new ValueTuple<Type, string>(
                    clientData.GetStructType(), clientData.GetClientDataName()))
                .Distinct()
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            typeToRequest = serviceProvider.GetServices<IDataListener>()
                .Select((request, index) => new ValueTuple<IDataListener, REQUEST>(request, (REQUEST)(index + ENUM_DYNAMIC_START)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
//TODO: check that the struct is also registered

            eventToEnum = serviceProvider.GetServices<IEvent>()
                .Select((e, index) => new ValueTuple<IEvent, EVENT>(e, (EVENT)(index + ENUM_DYNAMIC_START)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            notificationsToEvent = serviceProvider.GetServices<IEventNotification>()
                .ToDictionary(en => en, en => eventToEnum[en.GetEvent()]);

            onConnectionHandlers = serviceProvider.GetServices<IOnSimConnection>();
            onSimStartedHandlers = serviceProvider.GetServices<IOnSimStarted>();
            onSimFrameHandlers = serviceProvider.GetServices<IOnSimFrame>();

            return this;
        }

        private void Handle_OnRecvOpen(SimConnect _, SIMCONNECT_RECV_OPEN data)
        {
            RegisterDataStructs();
            MapClientEvents();
            SetGroupPriorities();

            SubscribeToSystemEvent(EVENT.SimSystemState, "Sim");
            SubscribeToSystemEvent(EVENT.Frame, "frame");
            _logging!.LogInformation($"Requested SimStart subscription {GetLastSentPacketID()}");

            TriggerInitialRequests();
        }

        public void TriggerInitialRequests()
        {
            if (IsSimStarted == null)
                RequestSystemState(REQUEST.SimSystemState, "Sim");

            StopAutoRequests(); // Ensure we get fresh data.
            _logging!.LogDebug("Requesting initial data");
            foreach (IRequestDataOnOpen request in typeToRequest!.Keys.OfType<IRequestDataOnOpen>())
                RequestDataOnSimObject(request, request.GetInitialRequestPeriod());

            //TODO: can/should we convert the above into the below?
            foreach (var handler in onConnectionHandlers!)
                handler.OnConnection(this);
        }

        private void Handle_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            throw new NotImplementedException("Is it enough to trust this? Is there anything else we should clean up?");
        }

        private void RegisterDataStructs()
        {
            foreach (var type2Struct in typeToStruct!)
            {
                Type type = type2Struct.Key;
                STRUCT id = type2Struct.Value;
                string? clientDataName = null;
                if (typeToClientDataName?.TryGetValue(type, out clientDataName) == true)
                    RegisterClientDataStruct(clientDataName!, type, id);
                else
                    RegisterDataStruct(type, id);
            }
        }

        private void RegisterDataStruct(Type type, STRUCT id)
        {
            foreach (FieldInfo field in type.GetFields())
            {
                var dataField = field.GetCustomAttribute<SimVarAttribute>();
                if (dataField == null)
                    throw new NullReferenceException($"No SimVarAttribute for {type}.{field.Name}");
                AddToDataDefinition(id, dataField.Variable, dataField.Units,
                    dataField.Type, dataField.Epsilon, SIMCONNECT_UNUSED);
//System.Console.Error.WriteLine($"Registered field {type}.{field.Name} {GetLastSentPacketID()}");
            }
            GetType().GetMethod("RegisterDataDefineStruct")!.MakeGenericMethod(type)
                .Invoke(this, new object[] { id });
//System.Console.Error.WriteLine($"Registered struct {type} {GetLastSentPacketID()}");
        }

        private void RegisterClientDataStruct(string clientDataName, Type type, Enum id)
        {
            MapClientDataNameToID(clientDataName, id);
//System.Console.Error.WriteLine($"Mapped client data {clientDataName} to {id}: {GetLastSentPacketID()}");
            CreateClientData(id, (uint)Marshal.SizeOf(type), SIMCONNECT_CREATE_CLIENT_DATA_FLAG.DEFAULT);
//System.Console.Error.WriteLine($"Created client data for {type}: {GetLastSentPacketID()}");

            GetType().GetMethod("RegisterStruct")!.MakeGenericMethod(typeof(SIMCONNECT_RECV_CLIENT_DATA), type)
                .Invoke(this, new object[] { id });
//System.Console.Error.WriteLine($"Registered struct {type}: {GetLastSentPacketID()}");

            foreach (FieldInfo field in type.GetFields())
            {
                var clientVar = field.GetCustomAttribute<ClientVarAttribute>();
                if (clientVar == null)
                    throw new NullReferenceException($"No ClientVarAttribute for {type}.{field.Name}");
                var marshallAs = field.GetCustomAttribute<MarshalAsAttribute>();
                if (marshallAs == null)
                    throw new NullReferenceException($"No MarshalAsAttribute for {type}.{field.Name}");

                uint clientTypeOrSize;
                if (marshallAs.Value == UnmanagedType.I1 || marshallAs.Value == UnmanagedType.U1)
                    clientTypeOrSize = SimConnect.SIMCONNECT_CLIENTDATATYPE_INT8;
                else if (marshallAs.Value == UnmanagedType.I2 || marshallAs.Value == UnmanagedType.U2)
                    clientTypeOrSize = SimConnect.SIMCONNECT_CLIENTDATATYPE_INT16;
                else if (marshallAs.Value == UnmanagedType.I4 || marshallAs.Value == UnmanagedType.U4)
                    clientTypeOrSize = SimConnect.SIMCONNECT_CLIENTDATATYPE_INT32;
                else if (marshallAs.Value == UnmanagedType.I8 || marshallAs.Value == UnmanagedType.U8)
                    clientTypeOrSize = SimConnect.SIMCONNECT_CLIENTDATATYPE_INT64;
                else if (marshallAs.Value == UnmanagedType.R4)
                    clientTypeOrSize = SimConnect.SIMCONNECT_CLIENTDATATYPE_FLOAT32;
                else if (marshallAs.Value == UnmanagedType.R8)
                    clientTypeOrSize = SimConnect.SIMCONNECT_CLIENTDATATYPE_FLOAT64;
                else if (marshallAs.Value == UnmanagedType.ByValTStr)
                    clientTypeOrSize = (uint)marshallAs.SizeConst;
                else
                    throw new ArgumentException($"Can't infer type from {marshallAs.MarshalTypeRef}/{marshallAs.Value} for {type}.{field.Name}");

                AddToClientDataDefinition(id, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO,
                    clientTypeOrSize, clientVar.Epsilon, SimConnect.SIMCONNECT_UNUSED);
//System.Console.Error.WriteLine($"Registered client field {type}.{field.Name}: {GetLastSentPacketID()}");
            }
        }

        private void MapClientEvents()
        {
            foreach (var eventToEnum in eventToEnum!)
            {
                MapClientEventToSimEvent(eventToEnum.Value, eventToEnum.Key.SimEvent());
//System.Console.Error.WriteLine($"Mapped client to sim event {eventToEnum.Key}: {GetLastSentPacketID()}");
            }
//System.Console.Error.WriteLine($"... and now notifications to events...");
            foreach (var notificationToEvent in notificationsToEvent!)
            {
                AddClientEventToNotificationGroup(GROUP.JUST_MASKABLE, notificationToEvent.Value, true);
//System.Console.Error.WriteLine($"Added {notificationToEvent.Key} to {notificationToEvent.Value}: {GetLastSentPacketID()}");
            }
        }

        private void SetGroupPriorities()
        {
            //TODO: Avoid doing this if there aren't any (at the time of writing they'd all gone to standalone WASM.)
            SetNotificationGroupPriority(GROUP.JUST_MASKABLE, SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE);
//System.Console.Error.WriteLine($"Set group priorities: {GetLastSentPacketID()}");
        }

        public void SendDataOnSimObject<StructType>(StructType data)
            where StructType : struct
        {
            STRUCT id = typeToStruct![typeof(StructType)];
            if (typeToClientDataName?.ContainsKey(data.GetType()) == true)
            {
                SetClientData(id, id, SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT, 0, data);
            }
            else
            {
                SetDataOnSimObject(id, SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, data);
            }
            //System.Console.Error.WriteLine($"Set data of type {data.GetType()} with id {id}: {GetLastSentPacketID()}");
        }

        private delegate string EventSender(EVENT @event, GROUP group, SIMCONNECT_EVENT_FLAG flags);

        public void SendEventEx1(IEvent eventToSend, uint data0 = 0u, uint data1 = 0u, uint data2 = 0u, uint data3 = 0u, uint data4 = 0u, SIMCONNECT_EVENT_FLAG flags = 0)
        {
            ActuallySendEvent(eventToSend, flags, (@event, group, flags) =>
            {
                TransmitClientEvent_EX1(SIMCONNECT_OBJECT_ID_USER, @event, group, flags, data0, data1, data2, data3, data4);
                return $"{data0},{data1}";
            });
        }

        public void SendEvent(IEvent eventToSend, uint data = 0u, SIMCONNECT_EVENT_FLAG flags = 0)
        {
            ActuallySendEvent(eventToSend, flags, (@event, group, flags) =>
            {
                TransmitClientEvent(SIMCONNECT_OBJECT_ID_USER, @event, data, group, flags);
                return $"{data}";
            });
        }

        private void ActuallySendEvent(IEvent eventToSend, SIMCONNECT_EVENT_FLAG flags, EventSender sender)
        {
            EVENT @event = eventToEnum![eventToSend];
            GROUP? group = notificationsToEvent!
                .Where(candidate => candidate.Value == @event)
                .Select(_ => GROUP.JUST_MASKABLE)
                .Distinct()
                .Cast<GROUP?>()
                .DefaultIfEmpty(null)
                .Single();
            if (group == null)
            {
                group = (GROUP)SIMCONNECT_GROUP_PRIORITY_HIGHEST;
                flags |= SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY;
            }
            string dataLog = sender.Invoke(@event, (GROUP)group, flags);
_logging?.LogDebug($"event {eventToSend}/{@event} group {group} data {dataLog} flags {flags}: {GetLastSentPacketID()}");
        }

        public void RequestDataOnSimObject(IDataListener data, Enum period)
        {
            REQUEST request = typeToRequest![data];
            STRUCT structId = typeToStruct![data.GetStructType()];
            if (typeToClientDataName?.ContainsKey(data.GetStructType()) == true)
            {
                SIMCONNECT_CLIENT_DATA_PERIOD clientPeriod = (SIMCONNECT_CLIENT_DATA_PERIOD)period;
                SIMCONNECT_CLIENT_DATA_REQUEST_FLAG flag = clientPeriod == SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET
                    ? SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.CHANGED
                    : SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.DEFAULT;
                RequestClientData(structId, request, structId, clientPeriod, flag, 0, 0, 0);
            }
            else
            {
                SIMCONNECT_PERIOD simPeriod = (SIMCONNECT_PERIOD)period;
                SIMCONNECT_DATA_REQUEST_FLAG flag = simPeriod == SIMCONNECT_PERIOD.ONCE
                                ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                                : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
                RequestDataOnSimObject(request, structId, SIMCONNECT_OBJECT_ID_USER, simPeriod, flag, 0, 0, 0);
            }
//System.Console.Error.WriteLine($"Get data on {data} period {period}: {GetLastSentPacketID()}");
        }

        private void Handle_OnRecvSimobjectData(SimConnect _, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            REQUEST request = (REQUEST)data.dwRequestID;
            var listener = typeToRequest!
                .Where(candidate => candidate.Value == request)
                .Select(candidate => candidate.Key)
                .Single();
            _logging!.LogDebug($"Received {request} for {listener} via OnRecvSimobjectData");
            listener.Process(this, data.dwData[0]);
        }

        private void Handle_OnRecvEventFrame(SimConnect simConnect, SIMCONNECT_RECV_EVENT_FRAME data)
        {
            EVENT e = (EVENT)data.uEventID;
            if (e != EVENT.Frame)
                return;
            foreach (var handler in onSimFrameHandlers!)
                handler.OnFrame(this, data);
        }

        private void Handle_OnRecvEvent(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            EVENT e = (EVENT)data.uEventID;
            if (e == EVENT.SimSystemState)
            {
                HandleSimSystemStateEvent(simConnect, data);
            }
            else
            {
                IEnumerable<IEventNotification> notifications = notificationsToEvent!
                    .Where(candidate => e == candidate.Value)
                    .Select(candidate => candidate.Key);
_logging!.LogDebug($"Received {e} for {String.Join(", ", notifications)}: {Convert.ToString(data.dwData, 16)} {(int)data.dwData}s (of {data.dwSize})"
    + $" Group ID {(GROUP)data.uGroupID} with ID {data.dwID} and version {data.dwVersion}");
                foreach (IEventNotification notification in notifications)
                {
                    notification.OnRecieve(this, data);
                }
            }
        }

        private void StopAutoRequests()
        {
            foreach (IRequestDataOnOpen request in typeToRequest!.Keys.OfType<IRequestDataOnOpen>())
                RequestDataOnSimObject(request, SIMCONNECT_PERIOD.NEVER);
        }

        private void HandleSimSystemStateEvent(SimConnect _, SIMCONNECT_RECV_EVENT data)
        {
            if ((IsSimStarted = (data.dwData == 1)) == true)
            {
                RequestSystemState(REQUEST.AircraftLoaded, "AircraftLoaded");
                _logging!.LogInformation($"Requested AircraftLoaded {GetLastSentPacketID()}");
                OnSimIsRunning();
            }
            else
                StopAutoRequests();
        }

        private void Handle_OnRecvSystemState(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            _logging!.LogError($"Received system state {(REQUEST)data.dwRequestID} with {data.szString}");

            if (data.dwRequestID == (uint)REQUEST.SimSystemState && IsSimStarted == null) {
                if ((IsSimStarted = (data.dwInteger == 1)) == true)
                    OnSimIsRunning();
                else
                    aircraftFile = "";
            }
            else if (data.dwRequestID == (uint)REQUEST.AircraftLoaded)
            {
                var regex = new Regex(@"^SimObjects\\Airplanes\\[^\\]+\\aircraft.CFG$", RegexOptions.IgnoreCase);
                var match = regex.Match(data.szString);
                aircraftFile = match.Success ? match.Groups[1].Value : data.szString;
            }
        }

        private string aircraftFile;
        public string AircraftFile { get => aircraftFile; }

        private void OnSimIsRunning()
        {
            foreach (var handler in onSimStartedHandlers!)
                serializedExecutor!.Enqueue(handler.OnStarted);
            TriggerInitialRequests();
        }
    }
}
