using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DataAttribute : Attribute
    {
        public readonly Type DataType;
        public DataAttribute(Type DataType) { this.DataType = DataType; }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DataField : Attribute
    {
        public readonly string Variable;
        public readonly string Units;
        public readonly SIMCONNECT_DATATYPE Type;
        public readonly float Epsilon;
        public DataField(string variable, string units, SIMCONNECT_DATATYPE type, float epsilon)
        {
            Variable = variable;
            Units = units;
            Type = type; //TODO: do we need this, or can we infer it?
            Epsilon = epsilon;
        }
    }

    enum DATA {
        // Stuff intended for struct-based multiple value requests:
        [DataAttribute(typeof(ApData))] FCU_DATA = 69,
        [DataAttribute(typeof(ApHdgSelData))] AP_HDG_SEL,
        [DataAttribute(typeof(ApModeData))] AP_DATA,
        [DataAttribute(typeof(SpoilerData))] SPOILER_DATA,
        [DataAttribute(typeof(SpoilerHandle))] SPOILER_HANDLE,
    }
    enum REQUEST { FCU_DATA = 71, FCU_HDG_SEL, AP_DATA, MORE_SPOILER, LESS_SPOILER, }
    /*TODO: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.categoryattribute?view=netcore-3.1
      Way to identify specific things?
      Would we be better to have a whole class for events and their recievers which includes an ID generator? */
    enum EVENT { NONE = 42,
        DISARM_SPOILER, ARM_SPOILER, MORE_SPOILER, LESS_SPOILER,
        AP_SPEED_SLOT_SET, AP_SPD_UP, AP_SPD_DOWN,
        AP_HEADING_SLOT_SET, AP_HDG_RIGHT, AP_HDG_LEFT, AP_HEADING_BUG_SET,
        AP_ALTITUDE_SLOT_SET, AP_ALT_UP, AP_ALT_DOWN,
        AP_TOGGLE_LOC, AP_TOGGLE_APPR, AP_AUTOTHRUST_ARM, AP_TOGGLE_FD,
        FCU_VS_UP, FCU_VS_DOWN, FCU_VS_SET, FCU_VS_SLOT_SET, AP_PANEL_VS_ON
    }
    enum GROUP { SPOILERS = 13,
        PRIORITY_STANDARD = 1900000000 }

    // FCU - general set of things we receive.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApData
    {
        [DataField("AUTOPILOT AIRSPEED HOLD VAR", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.5f)]
        public double speedKnots; // Real range 100 -399 knots (Mach 0.10-0.99).
        [DataField("AUTOPILOT SPEED SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 speedSlot;
        // Correct for selected, but not writable. When the user is pre-selecting, remains on the managed number.
        [DataField("AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 heading; // Real range 000-359 (not 360!)
        [DataField("AUTOPILOT HEADING SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 headingSlot;
        // In selected mode, this is correct (but not writable).
        // In managed mode, it shows what the autopilot is really doing (which may be modified by constraints).
        // Have not yet found where the displayed panel value is (may not be available via SimConnect).
        [DataField("AUTOPILOT ALTITUDE LOCK VAR", "feet", SIMCONNECT_DATATYPE.INT32, 50f)]
        public Int32 altitude; // Real range 100-49000
        [DataField("AUTOPILOT ALTITUDE SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 altitudeSlot;
        [DataField("AUTOPILOT VERTICAL HOLD VAR", "Feet/minute", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vs;
        [DataField("AUTOPILOT VS SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vsSlot;
        //TODO: set V/S to 0 on push, and engage selected V/S on pull; after 0ing, subsequent turns are actioned immediately
        //TODO: V/S selector; real range ±6000ft/min in steps of 100, or ±9.9º in steps of 0.1º
        //TODO: SPD/MCH buton; is it implemented yet?
        //TODO: HDG-V/S TRK-FPA toggle, when it's implemented
        //TODO: metric alt - does this work in MSFS?
    };

    // Autopilot - things we receive.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApModeData
    {
        [DataField("AUTOPILOT FLIGHT DIRECTOR ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fdActive;
        [DataField("AUTOPILOT MASTER", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apMaster;
        [DataField("AUTOPILOT HEADING LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apHeadingHold;
        [DataField("AUTOPILOT APPROACH HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 approachHold;
        [DataField("AUTOPILOT NAV1 LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 nav1Hold;
        [DataField("AUTOPILOT GLIDESLOPE HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 gsHold;
        [DataField("AUTOPILOT ALTITUDE LOCK", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apAltHold;
        [DataField("AUTOPILOT VERTICAL HOLD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apVSHold;
        [DataField("AUTOPILOT THROTTLE ARM", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 autothrustArmed;
        [DataField("AUTOTHROTTLE ACTIVE", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 autothrustActive;
        //TODO: EXPED button, when it's implemented
    }

    // FCU - things we get when pulling Heading to Selected.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApHdgSelData
    {
        [DataField("PLANE HEADING DEGREES MAGNETIC", "degrees", SIMCONNECT_DATATYPE.INT32, 0f)]
        public UInt32 headingMagnetic;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerData
    {
        [DataField("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersHandlePosition;
        [DataField("SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersArmed;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerHandle
    {
        [DataField("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersHandlePosition;
    };

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
#if TRUE
            {
                AssemblyName aName = new AssemblyName("TempAssembly");
                AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

                ModuleBuilder mb = ab.DefineDynamicModule(aName.Name!);

                EnumBuilder eb = mb.DefineEnum("Elevation", TypeAttributes.Public, typeof(int));

                // Define two members; there don't seem to be any inherent limitations on their names!
                eb.DefineLiteral("$Low fat <wible>", 0);
                eb.DefineLiteral("$High 'no rules'", 1);

                // Create the type and save the assembly.
                TypeInfo finished = eb.CreateTypeInfo()!;
                Enum sampleValue = (Enum)Enum.GetValues(finished).GetValue(1);
                viewModel.GSToolTip = $"Dynamic enum {finished} with {sampleValue}";
            }
#endif
        }

        public void AttachWinow([DisallowNull] HwndSource hWndSource)
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
            RegisterDataStructs(simConnect);

            simConnect.RequestDataOnSimObject(REQUEST.FCU_DATA, DATA.FCU_DATA,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);

            // FCU things we send.

            simConnect.MapClientEventToSimEvent(EVENT.AP_SPEED_SLOT_SET, "SPEED_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_SPD_UP, "AP_SPD_VAR_INC");
            simConnect.MapClientEventToSimEvent(EVENT.AP_SPD_DOWN, "AP_SPD_VAR_DEC");

            simConnect.MapClientEventToSimEvent(EVENT.AP_HEADING_SLOT_SET, "HEADING_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_HDG_RIGHT, "HEADING_BUG_INC");
            simConnect.MapClientEventToSimEvent(EVENT.AP_HDG_LEFT, "HEADING_BUG_DEC");
            simConnect.MapClientEventToSimEvent(EVENT.AP_HEADING_BUG_SET, "HEADING_BUG_SET");

            simConnect.MapClientEventToSimEvent(EVENT.AP_ALTITUDE_SLOT_SET, "ALTITUDE_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_ALT_UP, "AP_ALT_VAR_INC");
            simConnect.MapClientEventToSimEvent(EVENT.AP_ALT_DOWN, "AP_ALT_VAR_DEC");

            simConnect.MapClientEventToSimEvent(EVENT.FCU_VS_SET, "AP_VS_VAR_SET_ENGLISH");
            simConnect.MapClientEventToSimEvent(EVENT.FCU_VS_SLOT_SET, "VS_SLOT_INDEX_SET");
            simConnect.MapClientEventToSimEvent(EVENT.AP_PANEL_VS_ON, "AP_PANEL_VS_ON");
            simConnect.MapClientEventToSimEvent(EVENT.FCU_VS_DOWN, "AP_VS_VAR_DEC");
            simConnect.MapClientEventToSimEvent(EVENT.FCU_VS_UP, "AP_VS_VAR_INC");

            simConnect.RequestDataOnSimObject(REQUEST.AP_DATA, DATA.AP_DATA,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);

            // Autopilot - commands we send

            //TODO: APPR on then LOC on leaves GS stuck on; we should request modes and react
            simConnect.MapClientEventToSimEvent(EVENT.AP_TOGGLE_LOC, "AP_LOC_HOLD");
            simConnect.MapClientEventToSimEvent(EVENT.AP_TOGGLE_APPR, "AP_APR_HOLD"); // Combines LOC and GS?
            simConnect.MapClientEventToSimEvent(EVENT.AP_AUTOTHRUST_ARM, "AUTO_THROTTLE_ARM");
            simConnect.MapClientEventToSimEvent(EVENT.AP_TOGGLE_FD, "TOGGLE_FLIGHT_DIRECTOR");

            // Spoilers: things we recieve.

            simConnect.MapClientEventToSimEvent(EVENT.MORE_SPOILER, "SPOILERS_TOGGLE");
            simConnect.MapClientEventToSimEvent(EVENT.LESS_SPOILER, "SPOILERS_ARM_TOGGLE");

            simConnect.AddClientEventToNotificationGroup(GROUP.SPOILERS, EVENT.MORE_SPOILER, true);
            simConnect.AddClientEventToNotificationGroup(GROUP.SPOILERS, EVENT.LESS_SPOILER, true);

            simConnect.SetNotificationGroupPriority(GROUP.SPOILERS, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE);

            // Spoilers: things we send (we've artificially partitioned these as I couldn't avoid hearing our own event).

            simConnect.MapClientEventToSimEvent(EVENT.ARM_SPOILER, "SPOILERS_ARM_ON");
            simConnect.MapClientEventToSimEvent(EVENT.DISARM_SPOILER, "SPOILERS_ARM_OFF");
        }

        private void RegisterDataStructs(SimConnect simConnect)
        {
            foreach (Enum? value in typeof(DATA).GetEnumValues())
            {
                var dataType = value!.GetAttribute<DataAttribute>().DataType;
                foreach (FieldInfo field in dataType.GetFields())
                {
                    var dataField = field.GetCustomAttribute<DataField>();
                    if (dataField == null) throw new NullReferenceException($"No DataField for {dataType}.{field.Name}");
                    simConnect.AddToDataDefinition(value, dataField.Variable, dataField.Units, dataField.Type, dataField.Epsilon, SimConnect.SIMCONNECT_UNUSED);
                }
                simConnect.GetType().GetMethod("RegisterDataDefineStruct")!.MakeGenericMethod(dataType)
                    .Invoke(simConnect, new object[] { value });
            }
        }

        private void OnRecvEvent(SimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            switch ((EVENT)data.uEventID) {
                case EVENT.LESS_SPOILER:
                    simConnect.RequestDataOnSimObject(REQUEST.LESS_SPOILER, DATA.SPOILER_DATA,
                        SimConnect.SIMCONNECT_OBJECT_ID_USER,
                        SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
                    break;
                case EVENT.MORE_SPOILER:
                    simConnect.RequestDataOnSimObject(REQUEST.MORE_SPOILER, DATA.SPOILER_DATA,
                        SimConnect.SIMCONNECT_OBJECT_ID_USER,
                        SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
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
                        SetData(DATA.SPOILER_HANDLE, new SpoilerHandle { spoilersHandlePosition = Math.Min(spoilerData.spoilersHandlePosition + 25, 100) });
                    break;
                case REQUEST.LESS_SPOILER:
                    spoilerData = (SpoilerData)data.dwData[0];
                    if (spoilerData.spoilersHandlePosition > 0)
                        SetData(DATA.SPOILER_HANDLE, new SpoilerHandle { spoilersHandlePosition = Math.Max(spoilerData.spoilersHandlePosition - 25, 0) });
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
                    simConnect?.RequestDataOnSimObject(REQUEST.FCU_HDG_SEL, DATA.AP_HDG_SEL,
                        SimConnect.SIMCONNECT_OBJECT_ID_USER,
                        SIMCONNECT_PERIOD.ONCE, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
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

        private void SetData(DATA data, object value)
        {
            simConnect?.SetDataOnSimObject(data, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, value);
        }

        private void SendEvent(EVENT eventToSend, uint data = 0u, bool slow = false, bool fast = false)
        {
            SIMCONNECT_EVENT_FLAG flags = SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY;
            if (slow) flags |= SIMCONNECT_EVENT_FLAG.SLOW_REPEAT_TIMER;
            if (fast) flags |= SIMCONNECT_EVENT_FLAG.FAST_REPEAT_TIMER;
            simConnect?.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, eventToSend, data, GROUP.PRIORITY_STANDARD, flags);
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
