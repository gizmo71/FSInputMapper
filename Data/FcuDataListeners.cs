using System;
using System.Collections.Generic;
using System.Text;

namespace FSInputMapper.Data
{

    [Singleton]
    public class FcuDataListenerLeft : DataListener<ApData>
    {

        private readonly FSIMViewModel viewModel;

        public FcuDataListenerLeft(FSIMViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override void Process(ApData fcuData)
        {
            viewModel.AirspeedManaged = fcuData.speedSlot == 2;
            viewModel.AutopilotAirspeed = fcuData.speedKnots;
            viewModel.HeadingManaged = fcuData.headingSlot == 2;
            viewModel.AutopilotHeading = fcuData.heading;
        }

    }

    [Singleton]
    public class FcuDataListenerRight : DataListener<ApData>
    {

        private readonly FSIMViewModel viewModel;

        public FcuDataListenerRight(FSIMViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override void Process(ApData fcuData)
        {
            viewModel.AltitudeManaged = fcuData.altitudeSlot == 2;
            viewModel.AutopilotAltitude = fcuData.altitude;
            viewModel.AutopilotVerticalSpeed = fcuData.vs;
            viewModel.VerticalSpeedManaged = fcuData.vsSlot == 2;
        }

    }

    [Singleton]
    public class FcuModeDataListener : DataListener<ApModeData>
    {

        private readonly FSIMViewModel viewModel;

        public FcuModeDataListener(FSIMViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override void Process(ApModeData fcuModeData)
        {
            viewModel.AutopilotLoc = fcuModeData.approachHold != 0 && fcuModeData.gsHold == 0;
            viewModel.AutopilotAppr = fcuModeData.approachHold != 0 && fcuModeData.gsHold != 0;
            viewModel.AutopilotGs = fcuModeData.gsHold != 0;
            viewModel.GSToolTip = $"FD {fcuModeData.fdActive} APPH {fcuModeData.approachHold} APM {fcuModeData.apMaster}"
                + $"\nHH {fcuModeData.apHeadingHold} NavH {fcuModeData.nav1Hold}"
                + $" AltH {fcuModeData.apAltHold} vsH {fcuModeData.apVSHold}"
                + $"\nATHR arm {fcuModeData.autothrustArmed} act {fcuModeData.autothrustActive}";
        }

        [Singleton]
        public class FcuHeadingSelectDataListener : DataListener<ApHdgSelData>
        {

            private readonly SimConnectAdapter simConnectAdapter;

            public FcuHeadingSelectDataListener(SimConnectAdapter simConnectAdapter)
            {
                this.simConnectAdapter = simConnectAdapter;
            }

            public override void Process(ApHdgSelData fcuHeadingSelectData)
            {
                simConnectAdapter.SendEvent(EVENT.AP_HEADING_SLOT_SET, 1u);
                simConnectAdapter.SendEvent(EVENT.AP_HEADING_BUG_SET, (uint)fcuHeadingSelectData.headingMagnetic);
            }

        }

    }

}
