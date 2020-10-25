using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using FSInputMapper.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    internal enum REQUEST { }
    internal enum STRUCT { }

    internal class SimConnectzmo : SimConnect
    {

        internal Dictionary<Type, STRUCT>? typeToStruct;
        internal Dictionary<IDataListener, REQUEST>? typeToRequest;

        internal SimConnectzmo(string szName, IntPtr hWnd, uint UserEventWin32)
            : base(szName, hWnd, UserEventWin32, null, 0)
        {
        }

    }

    [Singleton]
    public class SimConnectHolder
    {
        public SimConnect? SimConnect { get; internal set; }
    }

    [Singleton]
    public class SimConnectAdapter {

        private const int WM_USER_SIMCONNECT = 0x0402;

        private IntPtr hWnd;
        private readonly FSIMViewModel viewModel; //TODO: decouple
        private SimConnectHolder scHolder;
        private readonly IServiceProvider serviceProvider;

        public SimConnectAdapter(FSIMViewModel viewModel,
            IServiceProvider serviceProvider,
            SimConnectHolder scHolder)
        {
            this.viewModel = viewModel;
            this.serviceProvider = serviceProvider;
            this.scHolder = scHolder;
        }

        public void AttachWinow(HwndSource hWndSource)
        {
            this.hWnd = hWndSource.Handle;
            hWndSource.AddHook(WndProc);

            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();
        }

        private void Disconnect(Exception ex)
        {
            scHolder.SimConnect?.Dispose();
            scHolder.SimConnect = null;
            viewModel.ConnectionError = ex.Message;
        }

        private void Tick(object? sender, EventArgs e)
        {
            if (scHolder.SimConnect != null) return;
if (!viewModel.DebugText.StartsWith("fish"))
{
                viewModel.DebugText = "fish";
                foreach (var service in serviceProvider.GetServices<IData>())
                {
                    viewModel.DebugText += $"\n{service} -> {service.GetStructType()}";
                }
}
            try
            {
                Connect();
                viewModel.ConnectionError = null;
            }
            catch (COMException ex)
            {
                Disconnect(ex);
            }
        }

        private void Connect()
        {
            scHolder.SimConnect = new SimConnectzmo("Gizmo's FSInputMapper", hWnd, WM_USER_SIMCONNECT);
            scHolder.SimConnect.OnRecvException += (sc, e) => { throw new Exception($"SimConnect threw {e.dwException} from send {e.dwSendID}, index {e.dwIndex}"); };
            AssignIds((scHolder.SimConnect as SimConnectzmo)!);
            scHolder.SimConnect.OnRecvOpen += OnRecvOpen;
            scHolder.SimConnect.OnRecvQuit += OnRecvQuit;
            scHolder.SimConnect.OnRecvSimobjectData += OnRecvSimobjectData;
            scHolder.SimConnect.OnRecvEvent += OnRecvEvent;
        }

        private void AssignIds(SimConnectzmo simConnect)
        {
            simConnect.typeToStruct = serviceProvider.GetServices<IData>()
                .Select(candidate => candidate.GetStructType())
                .Distinct()
                .Select((structType, index) => new ValueTuple<Type, STRUCT>(structType, (STRUCT)(index + 1)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            simConnect.typeToRequest = serviceProvider.GetServices<IDataListener>()
                .Select((request, index) => new ValueTuple<IDataListener, REQUEST>(request, (REQUEST)(index + 1)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
            //TODO: check that the struct is also registered
        }

        private void OnRecvQuit(SimConnect simConnect, SIMCONNECT_RECV data)
        {
            Disconnect(new Exception("Sim exited"));
        }

        private void OnRecvOpen(SimConnect simConnect, SIMCONNECT_RECV_OPEN data)
        {
            SimConnectzmo sc = (simConnect as SimConnectzmo)!;

            RegisterDataStructs(sc);
            MapClientEvents(sc);
            SetGroupPriorities(sc);

            foreach (IRequestDataOnOpen request in sc.typeToRequest!.Keys
                .Where(candidate => candidate is IRequestDataOnOpen)
                .Select(data => (IRequestDataOnOpen) data))
            {
                simConnect.RequestDataOnSimObject(request as IDataListener, request.GetInitialRequestPeriod());
            }
        }

        private void RegisterDataStructs(SimConnectzmo sc)
        {
            foreach (var type2Struct in sc.typeToStruct!)
            {
                foreach (FieldInfo field in type2Struct.Key.GetFields())
                {
                    var dataField = field.GetCustomAttribute<SCStructFieldAttribute>();
                    if (dataField == null)
                    {
                        throw new NullReferenceException($"No DataField for {type2Struct.Key}.{field.Name}");
                    }
                    sc.AddToDataDefinition(type2Struct.Value, dataField.Variable, dataField.Units,
                        dataField.Type, dataField.Epsilon, SimConnect.SIMCONNECT_UNUSED);
                }
                sc.GetType().GetMethod("RegisterDataDefineStruct")!.MakeGenericMethod(type2Struct.Key)
                    .Invoke(scHolder.SimConnect, new object[] { type2Struct.Value });
            }
        }

        private void MapClientEvents(SimConnectzmo sc)
        {
            foreach (EVENT? e in Enum.GetValues(typeof(EVENT)))
            {
                var eventAttribute = e!.GetAttribute<EventAttribute>();
                sc.MapClientEventToSimEvent(e, eventAttribute.ClientEvent);
                var groupAttribute = e!.GetAttribute<EventGroupAttribute>();
                if (groupAttribute != null) {
                    sc.AddClientEventToNotificationGroup(groupAttribute.Group, e, groupAttribute.IsMaskable);
                }
            }
        }

        private void SetGroupPriorities(SimConnectzmo sc)
        {
            foreach (GROUP? group in Enum.GetValues(typeof(GROUP)))
            {
                var attr = group!.GetAttribute<GroupAttribute>();
                sc.SetNotificationGroupPriority(group, attr.Priority);
            }
        }

        //TODO: turn into different objects
        private void OnRecvEvent(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            // These are the ones in the notification group.
            switch ((EVENT)data.uEventID) {
                case EVENT.SPOILERS_ARM_TOGGLE:
                    scHolder.SimConnect?.RequestDataOnSimObject(serviceProvider.GetRequiredService<LessSpoilerListener>(),
                        SIMCONNECT_PERIOD.ONCE);
                    break;
                case EVENT.SPOILERS_TOGGLE:
                    scHolder.SimConnect?.RequestDataOnSimObject(serviceProvider.GetRequiredService<MoreSpoilerListener>(),
                        SIMCONNECT_PERIOD.ONCE);
                    break;
            }
        }

        private void OnRecvSimobjectData(SimConnect simConnect, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            REQUEST request = (REQUEST)data.dwRequestID;
            ((SimConnectzmo)simConnect).typeToRequest
                .Where(candidate => candidate.Value == request)
                .Select(candidate => candidate.Key)
                .SingleOrDefault()
                .Process(simConnect, data.dwData[0]);
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled) {
            if (iMsg == WM_USER_SIMCONNECT)
            {
                try
                {
                    scHolder.SimConnect?.ReceiveMessage();
                } catch (Exception ex) {
                    Disconnect(ex);
                }
            }

            return IntPtr.Zero;
        }
    }

}
