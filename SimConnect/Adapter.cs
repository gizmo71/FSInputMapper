using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    [Singleton]
    public class SimConnectAdapter {
        private const int WM_USER_SIMCONNECT = 0x0402;

        private IntPtr hWnd;
        private readonly FSIMViewModel viewModel;
        private SimConnect? simConnect;

        public SimConnectAdapter(FSIMViewModel viewModel, FSIMTriggerBus triggerBus)
        {
            this.viewModel = viewModel;
            triggerBus.OnTrigger += OnTrigger;
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
            simConnect?.Dispose();
            simConnect = null;
            viewModel.ConnectionError = ex.Message;
        }

        private void Tick(object? sender, EventArgs e)
        {
            if (simConnect != null) return;
            try
            {
                PopulateMappings();
                Connect();
                viewModel.ConnectionError = null;
            }
            catch (COMException ex)
            {
                Disconnect(ex);
            }
        }

        private Dictionary<Type, STRUCT> structs = new Dictionary<Type, STRUCT>();

        private void PopulateMappings()
        {
            if (structs.Count == 0)
            {
                structs = Assembly.GetEntryAssembly()!.DefinedTypes
                    .Where(candidate => candidate.GetCustomAttribute<SCStructAttribute>() != null)
                    .Select((candidate, index) => new ValueTuple<Type, STRUCT>(candidate, (STRUCT)(index + 1)))
                    .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
            }
        }

        private void Connect()
        {
            simConnect = new SimConnect("Gizmo's FSInputMapper", hWnd, WM_USER_SIMCONNECT, null, 0);
            simConnect.OnRecvOpen += OnRecvOpen;
            simConnect.OnRecvQuit += OnRecvQuit;
            simConnect.OnRecvSimobjectData += OnRecvSimobjectData;
            simConnect.OnRecvEvent += OnRecvEvent;
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

            RequestDataOnSimObject(REQUEST.FCU_DATA);
            RequestDataOnSimObject(REQUEST.AP_DATA);
        }

        private void RegisterDataStructs()
        {
            foreach (var type2Struct in structs)
            {
                foreach (FieldInfo field in type2Struct.Key.GetFields())
                {
                    var dataField = field.GetCustomAttribute<SCStructFieldAttribute>();
                    if (dataField == null)
                    {
                        throw new NullReferenceException($"No DataField for {type2Struct.Key}.{field.Name}");
                    }
                    simConnect?.AddToDataDefinition(type2Struct.Value, dataField.Variable, dataField.Units,
                        dataField.Type, dataField.Epsilon, SimConnect.SIMCONNECT_UNUSED);
                }
                simConnect?.GetType().GetMethod("RegisterDataDefineStruct")!.MakeGenericMethod(type2Struct.Key)
                    .Invoke(simConnect, new object[] { type2Struct.Value });
            }
        }

        private void MapClientEvents()
        {
            foreach (EVENT? e in Enum.GetValues(typeof(EVENT)))
            {
                var eventAttribute = e!.GetAttribute<EventAttribute>();
                simConnect?.MapClientEventToSimEvent(e, eventAttribute.ClientEvent);
                var groupAttribute = e!.GetAttribute<EventGroupAttribute>();
                if (groupAttribute != null) {
                    simConnect?.AddClientEventToNotificationGroup(groupAttribute.Group, e, groupAttribute.IsMaskable);
                }
            }
        }

        private void SetGroupPriorities()
        {
            foreach (GROUP? group in Enum.GetValues(typeof(GROUP)))
            {
                var attr = group!.GetAttribute<GroupAttribute>();
                simConnect?.SetNotificationGroupPriority(group, attr.Priority);
            }
        }

        private void OnRecvEvent(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            // These are the ones in the notification group.
            switch ((EVENT)data.uEventID) {
                case EVENT.LESS_SPOILER:
                    RequestDataOnSimObject(REQUEST.LESS_SPOILER);
                    break;
                case EVENT.MORE_SPOILER:
                    RequestDataOnSimObject(REQUEST.MORE_SPOILER);
                    break;
            }
        }

        private void OnRecvSimobjectData(SimConnect simConnect, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            switch ((REQUEST)data.dwRequestID)
            {
                case REQUEST.FCU_DATA:
                    var fcuData = (ApData)data.dwData[0];
                    viewModel.AirspeedManaged = fcuData.speedSlot == 2;
                    viewModel.AutopilotAirspeed = fcuData.speedKnots;
                    viewModel.HeadingManaged = fcuData.headingSlot == 2;
                    viewModel.AutopilotHeading = fcuData.heading;
                    viewModel.AltitudeManaged = fcuData.altitudeSlot == 2;
                    viewModel.AutopilotAltitude = fcuData.altitude;
                    viewModel.AutopilotVerticalSpeed = fcuData.vs;
                    viewModel.VerticalSpeedManaged = fcuData.vsSlot == 2;
                    break;
                case REQUEST.AP_DATA:
                    var apModeData = (ApModeData)data.dwData[0];
                    viewModel.AutopilotLoc = apModeData.approachHold != 0 && apModeData.gsHold == 0;
                    viewModel.AutopilotAppr = apModeData.approachHold != 0 && apModeData.gsHold != 0;
                    viewModel.AutopilotGs = apModeData.gsHold != 0;
                    viewModel.GSToolTip = $"FD {apModeData.fdActive} APPH {apModeData.approachHold} APM {apModeData.apMaster}"
                        + $"\nHH {apModeData.apHeadingHold} NavH {apModeData.nav1Hold} AltH {apModeData.apAltHold} vsH {apModeData.apVSHold}"
                        + $"\nATHR arm {apModeData.autothrustArmed} act {apModeData.autothrustActive}";
                    break;
                case REQUEST.FCU_HDG_SEL:
                    var apSpdSelData = (ApHdgSelData)data.dwData[0];
                    SendEvent(EVENT.AP_HEADING_SLOT_SET, 1u);
                    SendEvent(EVENT.AP_HEADING_BUG_SET, (uint)apSpdSelData.headingMagnetic);
                    break;
                case REQUEST.MORE_SPOILER:
                    var spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersArmed != 0)
                        SendEvent(EVENT.DISARM_SPOILER);
                    else if (spoilerData.spoilersHandlePosition < 100)
                        SetData<SpoilerHandle>(new SpoilerHandle { spoilersHandlePosition = Math.Min(spoilerData.spoilersHandlePosition + 25, 100) });
                    break;
                case REQUEST.LESS_SPOILER:
                    spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersHandlePosition > 0)
                        SetData<SpoilerHandle>(new SpoilerHandle { spoilersHandlePosition = Math.Max(spoilerData.spoilersHandlePosition - 25, 0) });
                    else if (spoilerData.spoilersArmed == 0)
                        SendEvent(EVENT.ARM_SPOILER);
                    break;
            }
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
                    RequestDataOnSimObject(REQUEST.FCU_HDG_SEL);
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

        private void RequestDataOnSimObject(REQUEST request)
        {
            var ra = request.GetAttribute<RequestAttribute>();
            STRUCT structId = structs[ra.DataType];
            simConnect?.RequestDataOnSimObject(request, structId, SimConnect.SIMCONNECT_OBJECT_ID_USER,
                ra.Period, ra.Flag, 0, 0, 0);
        }

        private void SetData<StructType>(StructType value)
        {
            STRUCT id = structs[value!.GetType()];
            simConnect?.SetDataOnSimObject(id, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, value);
        }

        private void SendEvent(EVENT eventToSend, uint data = 0u, bool slow = false, bool fast = false)
        {
            SIMCONNECT_EVENT_FLAG flags = SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY;
            if (slow) flags |= SIMCONNECT_EVENT_FLAG.SLOW_REPEAT_TIMER;
            if (fast) flags |= SIMCONNECT_EVENT_FLAG.FAST_REPEAT_TIMER;
            simConnect?.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, data,
                (GROUP)SimConnect.SIMCONNECT_GROUP_PRIORITY_STANDARD, flags);
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled) {
            if (iMsg == WM_USER_SIMCONNECT)
            {
                try
                {
                    simConnect?.ReceiveMessage();
                } catch (Exception ex) {
                    Disconnect(ex);
                }
            }

            return IntPtr.Zero;
        }
    }

}
