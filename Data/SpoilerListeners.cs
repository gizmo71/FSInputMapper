using System;
using System.Collections.Generic;
using System.Text;

namespace FSInputMapper.Data
{

    [Singleton]
    public class MoreSpoilerListener : DataListener<SpoilerData>
    {

        public override void Process(SimConnectAdapter adapter, SpoilerData spoilerData)
        {
            if (spoilerData.spoilersArmed != 0)
                adapter.SendEvent(EVENT.DISARM_SPOILER);
            else if (spoilerData.spoilersHandlePosition < 100)
                adapter.SetData<SpoilerHandle>(new SpoilerHandle { spoilersHandlePosition = Math.Min(spoilerData.spoilersHandlePosition + 25, 100) });
        }

    }

    [Singleton]
    public class LessSpoilerListener : DataListener<SpoilerData>
    {

        public override void Process(SimConnectAdapter adapter, SpoilerData spoilerData)
        {
            if (spoilerData.spoilersHandlePosition > 0)
                adapter.SetData<SpoilerHandle>(new SpoilerHandle { spoilersHandlePosition = Math.Max(spoilerData.spoilersHandlePosition - 25, 0) });
            else if (spoilerData.spoilersArmed == 0)
                adapter.SendEvent(EVENT.ARM_SPOILER);
        }

    }

}
