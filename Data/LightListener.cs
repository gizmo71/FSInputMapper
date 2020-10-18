using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [Singleton]
    public class LightListener : DataListener<LightData>
    {

        private readonly FSIMViewModel viewModel;

        public LightListener(FSIMViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override void Process(SimConnectAdapter _, LightData lightData)
        {
            viewModel.Strobes = lightData.strobe != 1 ? 2 : 0;
            viewModel.GSToolTip = $"Strobes/Switch {lightData.strobe}/{lightData.strobeSwich} Beacon {lightData.beacon} Wing {lightData.wing} Nav/Logo {lightData.nav}/{lightData.logo}"
                + $"\nRunway ?? Landing {lightData.landing} Nose {lightData.nose}"
                + $"\nMask {Convert.ToString(lightData.mask, 2).PadLeft(10, '0')} Mask On {Convert.ToString(lightData.maskOn, 2).PadLeft(10, '0')}";
        }

    }

}
