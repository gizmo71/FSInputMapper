using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuAltPulled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_PULL";
        public string GetId() => "fcuAltPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuAltPushed : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_PUSH";
        public string GetId() => "fcuAltPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuAltInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_INC";
    }

    [Component]
    public class FcultDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_DEC";
    }

    [Component]
    public class FcuAltDelta : ISettable<Int16>
    {
        private readonly FcuAltInc inc;
        private readonly FcultDec dec;

        public FcuAltDelta(IServiceProvider sp)
        {
            inc = sp.GetRequiredService<FcuAltInc>();
            dec = sp.GetRequiredService<FcultDec>();
        }

        public string GetId() => "fcuAltDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            while (value != 0)
            {
                simConnect.SendEvent(value < 0 ? dec : inc);
                value -= (short)Math.Sign(value);
            }
        }
    }

    [Component]
    public class FcuAltIncrement : ISettable<uint>, IEvent
    {
        public string GetId() => "fcuAltIncrement";
        public string SimEvent() => "A32NX.FCU_ALT_INCREMENT_SET";
        public void SetInSim(ExtendedSimConnect simConnect, uint value) => simConnect.SendEvent(this, value);
    }
}
