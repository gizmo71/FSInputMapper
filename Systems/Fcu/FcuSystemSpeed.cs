using System;
using System.Runtime.InteropServices;
using FSInputMapper.Data;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Fcu
{

    [Singleton]
    public class FcuSpeedModeSet : IEvent { public string SimEvent() { return "SPEED_SLOT_INDEX_SET"; } }

    [Singleton]
    public class FcuSpeedIncrease : IEvent { public string SimEvent() { return "AP_SPD_VAR_INC"; } }

    [Singleton]
    public class FcuSpeedDecrease : IEvent { public string SimEvent() { return "AP_SPD_VAR_DEC"; } }

    [Singleton]
    public class FcuSpeedSet : IEvent { public string SimEvent() { return "AP_SPD_VAR_SET"; } }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApSpeedData
    {
        [SCStructField("AUTOPILOT AIRSPEED HOLD VAR", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.1f)]
        public double speedKnots; // Real range 100 -399 knots (Mach 0.10-0.99).
        [SCStructField("AUTOPILOT SPEED SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 speedSlot;
    }

    [Singleton]
    public class FcuSpeedListener : DataListener<ApSpeedData>, IRequestDataOnOpen
    {

        private readonly FcuSystem fcuSystem;

        public FcuSpeedListener(FcuSystem fcuSystem)
        {
            this.fcuSystem = fcuSystem;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.SIM_FRAME;
        }

        public override void Process(SimConnect _, ApSpeedData data)
        {
            fcuSystem.SpeedSelected = data.speedSlot == 1;
            fcuSystem.Speed = data.speedKnots;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuSpeedSelectedData
    {
        [SCStructField("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.INT32, 0f)]
        public UInt32 indicatedKnots;
        [SCStructField("AIRSPEED MACH", "Mach", SIMCONNECT_DATATYPE.FLOAT32, 0f)]
        public float indicatedMach;
    };

    [Singleton]
    public class FcuSpeedSelectedListener : DataListener<FcuSpeedSelectedData>
    {

        private readonly FcuSpeedModeSet speedModeSet;
        private readonly FcuSpeedSet speedSet;

        public FcuSpeedSelectedListener(FcuSpeedModeSet speedModeSet, FcuSpeedSet speedSet)
        {
            this.speedModeSet = speedModeSet;
            this.speedSet = speedSet;
        }

        public override void Process(SimConnect simConnect, FcuSpeedSelectedData data)
        {
            simConnect.SendEvent(speedSet, data.indicatedKnots);
            simConnect.SendEvent(speedModeSet, 1u);
        }

    }

    public partial class FcuSystem
    {

        private readonly FcuSpeedSelectedListener speedSelectedListener;
        private readonly FcuSpeedModeSet speedModeSet;
        private readonly FcuSpeedIncrease speedIncrease;
        private readonly FcuSpeedDecrease speedDecrease;

        public void SetSpeedSelected(bool isSelected)
        {
            if (isSelected)
                scHolder.SimConnect?.RequestDataOnSimObject(speedSelectedListener, SIMCONNECT_PERIOD.ONCE);
            else
                scHolder.SimConnect?.SendEvent(speedModeSet, 2u);
        }

        private bool speedSelected;
        public bool SpeedSelected
        {
            get { return speedSelected; }
            internal set { if (speedSelected != value) { speedSelected = value; OnPropertyChange(); } }
        }

        private double speed;
        public double Speed
        {
            get { return speed; }
            internal set { if (speed != value) { speed = value; OnPropertyChange(); } }
        }

        public void SpeedChange(int delta)
        {
            bool isSlow = Math.Abs(delta) == 1;
            IEvent eventToSend = delta < 0 ? (IEvent)speedDecrease : speedIncrease;
            scHolder.SimConnect?.SendEvent(eventToSend, slow: isSlow, fast: !isSlow);
        }

    }

}
