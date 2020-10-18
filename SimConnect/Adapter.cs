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
        private readonly FSIMViewModel viewModel;
        private SimConnectHolder scHolder;
        private readonly IServiceProvider serviceProvider;

        public SimConnectAdapter(FSIMViewModel viewModel,
            FSIMTriggerBus triggerBus,
            IServiceProvider serviceProvider,
            SimConnectHolder scHolder)
        {
            this.viewModel = viewModel;
            triggerBus.OnTrigger += OnTrigger;
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
            scHolder.SimConnect.OnRecvException += (sc, e) => { throw new Exception($"SimConnect threw {e.dwException}"); };
            AssignStructIds();
            AssignRequestIds();
            scHolder.SimConnect.OnRecvOpen += OnRecvOpen;
            scHolder.SimConnect.OnRecvQuit += OnRecvQuit;
            scHolder.SimConnect.OnRecvSimobjectData += OnRecvSimobjectData;
            scHolder.SimConnect.OnRecvEvent += OnRecvEvent;
        }

        private void OnRecvQuit(SimConnect simConnect, SIMCONNECT_RECV data)
        {
            Disconnect(new Exception("Sim exited"));
        }

        private void OnRecvOpen(SimConnect simConnect, SIMCONNECT_RECV_OPEN data)
        {
            RegisterDataStructs();
            MapClientEvents();
            SetGroupPriorities();

            RequestDataOnSimObject(serviceProvider.GetRequiredService<FcuDataListener>(), SIMCONNECT_PERIOD.SIM_FRAME);
            RequestDataOnSimObject(serviceProvider.GetRequiredService<FcuModeDataListener>(), SIMCONNECT_PERIOD.SIM_FRAME);
            RequestDataOnSimObject(serviceProvider.GetRequiredService<LightListener>(), SIMCONNECT_PERIOD.SIM_FRAME);
        }

        private void AssignStructIds()
        {
            (scHolder.SimConnect! as SimConnectzmo)!.typeToStruct = serviceProvider.GetServices<IData>()
                .Select(candidate => candidate.GetStructType())
                .Distinct()
                .Select((structType, index) => new ValueTuple<Type, STRUCT>(structType, (STRUCT)(index + 1)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        private void AssignRequestIds()
        {
            (scHolder.SimConnect! as SimConnectzmo)!.typeToRequest = serviceProvider.GetServices<IDataListener>()
                .Select((request, index) => new ValueTuple<IDataListener, REQUEST>(request, (REQUEST)(index + 1)))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        private void RegisterDataStructs()
        {
            foreach (var type2Struct in (scHolder.SimConnect! as SimConnectzmo)!.typeToStruct!)
            {
                foreach (FieldInfo field in type2Struct.Key.GetFields())
                {
                    var dataField = field.GetCustomAttribute<SCStructFieldAttribute>();
                    if (dataField == null)
                    {
                        throw new NullReferenceException($"No DataField for {type2Struct.Key}.{field.Name}");
                    }
                    scHolder.SimConnect?.AddToDataDefinition(type2Struct.Value, dataField.Variable, dataField.Units,
                        dataField.Type, dataField.Epsilon, SimConnect.SIMCONNECT_UNUSED);
                }
                scHolder.SimConnect?.GetType().GetMethod("RegisterDataDefineStruct")!.MakeGenericMethod(type2Struct.Key)
                    .Invoke(scHolder.SimConnect, new object[] { type2Struct.Value });
            }
        }

        private void MapClientEvents()
        {
            foreach (EVENT? e in Enum.GetValues(typeof(EVENT)))
            {
                var eventAttribute = e!.GetAttribute<EventAttribute>();
                scHolder.SimConnect?.MapClientEventToSimEvent(e, eventAttribute.ClientEvent);
                var groupAttribute = e!.GetAttribute<EventGroupAttribute>();
                if (groupAttribute != null) {
                    scHolder.SimConnect?.AddClientEventToNotificationGroup(groupAttribute.Group, e, groupAttribute.IsMaskable);
                }
            }
        }

        private void SetGroupPriorities()
        {
            foreach (GROUP? group in Enum.GetValues(typeof(GROUP)))
            {
                var attr = group!.GetAttribute<GroupAttribute>();
                scHolder.SimConnect?.SetNotificationGroupPriority(group, attr.Priority);
            }
        }

        private void OnRecvEvent(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            // These are the ones in the notification group.
            switch ((EVENT)data.uEventID) {
                case EVENT.LESS_SPOILER:
                    RequestDataOnSimObject(serviceProvider.GetRequiredService<LessSpoilerListener>(),
                        SIMCONNECT_PERIOD.ONCE);
                    break;
                case EVENT.MORE_SPOILER:
                    RequestDataOnSimObject(serviceProvider.GetRequiredService<MoreSpoilerListener>(),
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
                .Process(this, data.dwData[0]);
        }

        private void OnTrigger(object? sender, FSIMTriggerArgs e)
        {
            switch (e.What)
            {
                case FSIMTrigger.SPD_MAN:
                    //TODO: send something to unset the locked selected value
                    SendEvent(EVENT.AP_SPEED_SLOT_SET, 2u);
                    break;
                case FSIMTrigger.SPD_SEL:
                    SendEvent(EVENT.AP_SPEED_SLOT_SET, 1u);
                    break;
                case FSIMTrigger.SPD_1_FASTER:
                    SendEvent(EVENT.AP_SPD_UP, fast: true);
                    break;
                case FSIMTrigger.SPD_10_FASTER:
                    SendEvent(EVENT.AP_SPD_UP, slow: true);
                    break;
                case FSIMTrigger.SPD_1_SLOWER:
                    SendEvent(EVENT.AP_SPD_DOWN, fast: true);
                    break;
                case FSIMTrigger.SPD_10_SLOWER:
                    SendEvent(EVENT.AP_SPD_DOWN, slow: true);
                    break;
                case FSIMTrigger.HDG_MAN:
//TODO: if AUTOPILOT APPROACH HOLD is TRUE, ignore the push
                    SendEvent(EVENT.AP_HEADING_SLOT_SET, 2u);
                    break;
                case FSIMTrigger.HDG_SEL:
//TODO: if AUTOPILOT APPROACH HOLD is TRUE, ignore the pull
                    RequestDataOnSimObject(serviceProvider.GetRequiredService<FcuHeadingSelectDataListener>(),
                        SIMCONNECT_PERIOD.ONCE);
                    break;
                case FSIMTrigger.HDG_RIGHT_1:
                    SendEvent(EVENT.AP_HDG_RIGHT, fast: true);
                    break;
                case FSIMTrigger.HDG_RIGHT_10:
                    SendEvent(EVENT.AP_HDG_RIGHT, slow: true);
                    break;
                case FSIMTrigger.HDG_LEFT_1:
                    SendEvent(EVENT.AP_HDG_LEFT, fast: true);
                    break;
                case FSIMTrigger.HDG_LEFT_10:
                    SendEvent(EVENT.AP_HDG_LEFT, slow: true);
                    break;
                case FSIMTrigger.ALT_MAN:
                    SendEvent(EVENT.AP_ALTITUDE_SLOT_SET, 2u);
                    break;
                case FSIMTrigger.ALT_SEL:
                    SendEvent(EVENT.AP_ALTITUDE_SLOT_SET, 1u);
                    break;
                case FSIMTrigger.ALT_UP_1000:
                    SendEvent(EVENT.AP_ALT_UP, 1000u);
                    break;
                case FSIMTrigger.ALT_UP_100:
                    SendEvent(EVENT.AP_ALT_UP, 100u);
                    break;
                case FSIMTrigger.ALT_DOWN_1000:
                    SendEvent(EVENT.AP_ALT_DOWN, 1000u);
                    break;
                case FSIMTrigger.ALT_DOWN_100:
                    SendEvent(EVENT.AP_ALT_DOWN, 100u);
                    break;
                case FSIMTrigger.VS_STOP:
                    //TODO: A320_Neo_FCU.js says AP_PANEL_ALTITUDE_HOLD=1 and AP_PANEL_VS_ON=1
                    SendEvent(EVENT.FCU_VS_SET, 0);
                    goto vsSel;
                case FSIMTrigger.VS_SEL:
                //TODO: if in idle descent, set AP_VS_VAR_SET_ENGLISH twice with different parameters and the current rate, then send AP_PANEL_VS_ON=1
                //TODO: else, set AP_VS_VAR_SET_ENGLISH to current value, and then send VS_SLOT_INDEX_SET=AP_PANEL_VS_ON=1
                vsSel:
                    SendEvent(EVENT.FCU_VS_SLOT_SET, 1u);
                    goto vsPanelOn;
                case FSIMTrigger.VS_DOWN:
                    SendEvent(EVENT.FCU_VS_DOWN);
                    goto vsPanelOn;
                //TODO: if not yet active, on first turn set AP_VS_VAR_SET_ENGLISH starting value from VERTICAL SPEED
                //TODO: else, set AP_VS_VAR_SET_ENGLISH to new selection, and then send VS_SLOT_INDEX_SET=AP_PANEL_VS_ON=1
                case FSIMTrigger.VS_UP:
                    SendEvent(EVENT.FCU_VS_UP);
                vsPanelOn:
                    SendEvent(EVENT.AP_PANEL_VS_ON);
                    break;
                case FSIMTrigger.TOGGLE_LOC_MODE:
                    if (viewModel.AutopilotAppr)
                        SendEvent(EVENT.AP_TOGGLE_APPR);
                    SendEvent(EVENT.AP_TOGGLE_LOC);
                    break;
                case FSIMTrigger.TOGGLE_APPR_MODE:
                    if (viewModel.AutopilotLoc)
                        SendEvent(EVENT.AP_TOGGLE_LOC);
                    SendEvent(EVENT.AP_TOGGLE_APPR);
                    break;
            }
        }

        private void RequestDataOnSimObject(IDataListener data, SIMCONNECT_PERIOD period)
        {
            if (scHolder.SimConnect is SimConnectzmo sc)
            {
                REQUEST request = sc.typeToRequest![data];
                STRUCT structId = sc.typeToStruct![data.GetStructType()];
                SIMCONNECT_DATA_REQUEST_FLAG flag = period == SIMCONNECT_PERIOD.ONCE
                                ? SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT
                                : SIMCONNECT_DATA_REQUEST_FLAG.CHANGED;
                sc.RequestDataOnSimObject(request, structId,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER, period, flag, 0, 0, 0);
            }
        }

        public void SendEvent(EVENT eventToSend, uint data = 0u, bool slow = false, bool fast = false)
        {
            SIMCONNECT_EVENT_FLAG flags = SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY;
            if (slow) flags |= SIMCONNECT_EVENT_FLAG.SLOW_REPEAT_TIMER;
            if (fast) flags |= SIMCONNECT_EVENT_FLAG.FAST_REPEAT_TIMER;
            scHolder.SimConnect?.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, data,
                (GROUP)SimConnect.SIMCONNECT_GROUP_PRIORITY_STANDARD, flags);
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
