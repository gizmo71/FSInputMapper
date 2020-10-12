using System;
using System.Collections.Generic;
using System.Text;

namespace FSInputMapper.Data
{

    [Singleton]
    public class FcuDataListener : Data<ApData>
    {

        private readonly FSIMViewModel viewModel;

        public FcuDataListener(FSIMViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override void Process(ApData fcuData)
        {
            viewModel.AirspeedManaged = fcuData.speedSlot == 2;
            viewModel.AutopilotAirspeed = fcuData.speedKnots;
            viewModel.HeadingManaged = fcuData.headingSlot == 2;
            viewModel.AutopilotHeading = fcuData.heading;
            viewModel.AltitudeManaged = fcuData.altitudeSlot == 2;
            viewModel.AutopilotAltitude = fcuData.altitude;
            viewModel.AutopilotVerticalSpeed = fcuData.vs;
            viewModel.VerticalSpeedManaged = fcuData.vsSlot == 2;
        }
    }
}
