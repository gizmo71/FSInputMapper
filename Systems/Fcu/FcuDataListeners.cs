using System;
using System.Collections.Generic;
using System.Text;
using FSInputMapper.Systems.Fcu;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [Singleton]
    public class FcuDataListener : DataListener<ApData>, IRequestDataOnOpen
    {

        private readonly FSIMViewModel viewModel;
        private readonly FcuSystem fcuSystem;

        public FcuDataListener(FSIMViewModel viewModel, FcuSystem fcuSystem)
        {
            this.viewModel = viewModel;
            this.fcuSystem = fcuSystem;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.SIM_FRAME;
        }

        public override void Process(SimConnect _, ApData fcuData)
        {
            viewModel.AirspeedManaged = fcuData.speedSlot == 2;
            fcuSystem.Speed = fcuData.speedKnots;
            viewModel.HeadingManaged = fcuData.headingSlot == 2;
            viewModel.AutopilotHeading = fcuData.heading;
            viewModel.AltitudeManaged = fcuData.altitudeSlot == 2;
            viewModel.AutopilotAltitude = fcuData.altitude;
            viewModel.AutopilotVerticalSpeed = fcuData.vs;
            viewModel.VerticalSpeedManaged = fcuData.vsSlot == 2;
        }

    }

    [Singleton]
    public class FcuModeDataListener : DataListener<ApModeData>, IRequestDataOnOpen
    {

        private readonly FSIMViewModel viewModel;

        public FcuModeDataListener(FSIMViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.SIM_FRAME;
        }

        public override void Process(SimConnect _, ApModeData fcuModeData)
        {
            viewModel.AutopilotLoc = fcuModeData.approachHold != 0 && fcuModeData.gsHold == 0;
            viewModel.AutopilotAppr = fcuModeData.approachHold != 0 && fcuModeData.gsHold != 0;
            viewModel.AutopilotGs = fcuModeData.gsHold != 0;
#if FALSE
            viewModel.GSToolTip = $"FD {fcuModeData.fdActive} APM {fcuModeData.apMaster}"
                + $"\nHH {fcuModeData.apHeadingHold} NavH {fcuModeData.nav1Hold}"
                + $" AltH {fcuModeData.apAltHold} vsH {fcuModeData.apVSHold}"
                + $"\nATHR arm {fcuModeData.autothrustArmed} act {fcuModeData.autothrustActive}";
#endif
        }

    }

    [Singleton]
    public class FcuHeadingSelectDataListener : DataListener<ApHdgSelData>
    {

        public override void Process(SimConnect simConnect, ApHdgSelData fcuHeadingSelectData)
        {
            simConnect.SendEvent(EVENT.AP_HEADING_SLOT_SET, 1u);
            simConnect.SendEvent(EVENT.AP_HEADING_BUG_SET, (uint)fcuHeadingSelectData.headingMagnetic);
        }

    }

}
