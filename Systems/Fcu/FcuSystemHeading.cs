using System;
using System.Runtime.InteropServices;
using FSInputMapper.Data;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Fcu
{

    #region Events

    [Singleton]
    public class FcuHeadingModeSet : IEvent { public string SimEvent() { return "HEADING_SLOT_INDEX_SET"; } }

    [Singleton]
    public class FcuHeadingIncrease : IEvent { public string SimEvent() { return "HEADING_BUG_INC"; } }

    [Singleton]
    public class FcuHeadingDecrease : IEvent { public string SimEvent() { return "HEADING_BUG_DEC"; } }

    [Singleton]
    public class FcuHeadingBugSet : IEvent { public string SimEvent() { return "HEADING_BUG_SET"; } }

    #endregion
    #region Sim State Listener

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuHeadingData
    {
        // Correct for selected, but not writable. When the user is pre-selecting, remains on the managed number.
        [SCStructField("AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 heading; // Real range 000-359 (not 360!)
        [SCStructField("AUTOPILOT HEADING SLOT INDEX", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 headingSlot;
    }

    [Singleton]
    public class FcuHeadingListener : DataListener<FcuHeadingData>, IRequestDataOnOpen
    {

        private readonly FcuSystem fcuSystem;

        public FcuHeadingListener(FcuSystem fcuSystem)
        {
            this.fcuSystem = fcuSystem;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.SIM_FRAME;
        }

        public override void Process(SimConnect _, FcuHeadingData data)
        {
            fcuSystem.HeadingSelected = data.headingSlot == 1;
            fcuSystem.Heading = data.heading;
_.Set6dof(data.heading);
        }

    }

    #endregion
    #region Selected Pull

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuHeadingSelectedData
    {
        [SCStructField("PLANE HEADING DEGREES MAGNETIC", "degrees", SIMCONNECT_DATATYPE.INT32, 0f)]
        public UInt32 headingMagnetic;
    };

    [Singleton]
    public class FcuHeadingSelectedListener : DataListener<FcuHeadingSelectedData>
    {

        private readonly FcuHeadingModeSet headingModeSet;
        private readonly FcuHeadingBugSet headingBugSet;

        public FcuHeadingSelectedListener(FcuHeadingModeSet headingModeSet, FcuHeadingBugSet headingBugSet)
        {
            this.headingModeSet = headingModeSet;
            this.headingBugSet = headingBugSet;
        }

        public override void Process(SimConnect simConnect, FcuHeadingSelectedData fcuHeadingSelectData)
        {
            simConnect.SendEvent(headingModeSet, 1u);
            simConnect.SendEvent(headingBugSet, (uint)fcuHeadingSelectData.headingMagnetic);
        }

    }

    #endregion

    public partial class FcuSystem
    {

        private readonly FcuHeadingSelectedListener headingSelectedListener;
        private readonly FcuHeadingModeSet headingModeSet;
        private readonly FcuHeadingDecrease headingDecrease;
        private readonly FcuHeadingIncrease headingIncrease;

        public void SetHeadingSelected(bool isSelected)
        {
            //TODO: if AUTOPILOT APPROACH HOLD is TRUE, ignore
            if (isSelected)
                scHolder.SimConnect?.RequestDataOnSimObject(headingSelectedListener, SIMCONNECT_PERIOD.ONCE);
            else
                scHolder.SimConnect?.SendEvent(headingModeSet, 2u);
        }

        private bool headingSelected;
        public bool HeadingSelected
        {
            get { return headingSelected; }
            internal set { if (headingSelected != value) { headingSelected = value; OnPropertyChange(); } }
        }

        private int heading;
        public int Heading
        {
            get { return heading; }
            internal set { if (heading != value) { heading = value; OnPropertyChange(); } }
        }

        public void HeadingChange(int delta)
        {
            bool isFast = Math.Abs(delta) == 1;
            IEvent eventToSend = delta < 0 ? (IEvent)headingDecrease : headingIncrease;
            scHolder.SimConnect?.SendEvent(eventToSend, slow: !isFast, fast: isFast);
        }

    }

}
