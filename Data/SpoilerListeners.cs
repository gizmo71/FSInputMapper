using System;
using System.Collections.Generic;
using System.Text;

namespace FSInputMapper.Data
{

    [Singleton]
    public class MoreSpoilerListener : DataListener<SpoilerData>
    {

        private readonly SimConnectAdapter simConnectAdapter;

        public MoreSpoilerListener(SimConnectAdapter simConnectAdapter)
        {
            this.simConnectAdapter = simConnectAdapter;
        }

        public override void Process(SpoilerData spoilerData)
        {
            if (spoilerData.spoilersArmed != 0)
                simConnectAdapter.SendEvent(EVENT.DISARM_SPOILER);
            else if (spoilerData.spoilersHandlePosition < 100)
                simConnectAdapter.SetData<SpoilerHandle>(new SpoilerHandle { spoilersHandlePosition = Math.Min(spoilerData.spoilersHandlePosition + 25, 100) });
        }

        public override bool Accept(REQUEST request) {
            return request == REQUEST.MORE_SPOILER;
        }

    }

    [Singleton]
    public class LessSpoilerListener : DataListener<SpoilerData>
    {

        private readonly SimConnectAdapter simConnectAdapter;

        public LessSpoilerListener(SimConnectAdapter simConnectAdapter)
        {
            this.simConnectAdapter = simConnectAdapter;
        }

        public override void Process(SpoilerData spoilerData)
        {
            if (spoilerData.spoilersHandlePosition > 0)
                simConnectAdapter.SetData<SpoilerHandle>(new SpoilerHandle { spoilersHandlePosition = Math.Max(spoilerData.spoilersHandlePosition - 25, 0) });
            else if (spoilerData.spoilersArmed == 0)
                simConnectAdapter.SendEvent(EVENT.ARM_SPOILER);
        }

        public override bool Accept(REQUEST request)
        {
            return request == REQUEST.LESS_SPOILER;
        }

    }

}
