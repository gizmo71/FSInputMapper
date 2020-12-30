using System;
using System.Runtime.InteropServices;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineData
    {
        [SCStructField("MASTER IGNITION SWITCH", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 mastIgnSw;
        [SCStructField("GENERAL ENG STARTER:0", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 genEngSt0;
        [SCStructField("GENERAL ENG STARTER:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 genEngSt1;
        [SCStructField("GENERAL ENG FUEL VALVE:0", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 genEngFV0;
        [SCStructField("GENERAL ENG FUEL VALVE:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 genEngFV1;
        [SCStructField("TURB ENG IGNITION SWITCH", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 turbEnIg;
        [SCStructField("TURB ENG MASTER STARTER SWITCH", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 tubEngMSS;
    };

    [Singleton]
    public class EngineListener : DataListener<EngineData>, IRequestDataOnOpen
    {

        private readonly DebugConsole dc;

        public EngineListener(DebugConsole dc)
        {
            this.dc = dc;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(SimConnect _, EngineData data)
        {
            dc.Text = $"MIS={data.mastIgnSw} GES {data.genEngSt0} {data.genEngSt1} GEFV {data.genEngSt0} {data.genEngSt1}"
                + $"\nTurbEng IS {data.turbEnIg} MSS {data.tubEngMSS}";
        }

    }

    [Singleton]
    public class ArmAutothrustListener : DataListener<AutothrustState>
    {
        private readonly AutothrustArmToggleEvent armEvent;

        public ArmAutothrustListener(AutothrustArmToggleEvent armEvent)
        {
            this.armEvent = armEvent;
        }

        public override void Process(SimConnect simConnect, AutothrustState armingState)
        {
            if (armingState.autothrustArmed == 0)
                simConnect.SendEvent(armEvent);
        }
    }

}
