using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuHeadingPulled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_HDG_PULL";
        public string GetId() => "fcuHeadingPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuHeadingPushed : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_HDG_PUSH";
        public string GetId() => "fcuHeadingPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuHeadingInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_HDG_INC";
    }

    [Component]
    public class FcuHeadingDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_HDG_DEC";
    }

    [Component]
    public class FcuHeadingDelta : ISettable<Int16>
    {
        private readonly FcuHeadingInc inc;
        private readonly FcuHeadingDec dec;

        public FcuHeadingDelta(IServiceProvider sp)
        {
            inc = sp.GetRequiredService<FcuHeadingInc>();
            dec = sp.GetRequiredService<FcuHeadingDec>();
        }

        public string GetId() => "fcuHeadingDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            while (value != 0)
            {
                simConnect.SendEvent(value < 0 ? dec : inc);
                value -= (short)Math.Sign(value);
            }
        }
    }
}
