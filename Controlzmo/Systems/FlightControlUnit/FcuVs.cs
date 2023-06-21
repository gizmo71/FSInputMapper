using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuVsPulled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_PULL";
        public string GetId() => "fcuVsPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuVsPushed : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_PUSH";
        public string GetId() => "fcuVsPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuVsInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_INC";
    }

    [Component]
    public class FcuVsDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_VS_DEC";
    }

    [Component]
    public class FcuVsDelta : ISettable<Int16>
    {
        private readonly FcuVsInc inc;
        private readonly FcuVsDec dec;

        public FcuVsDelta(IServiceProvider sp)
        {
            inc = sp.GetRequiredService<FcuVsInc>();
            dec = sp.GetRequiredService<FcuVsDec>();
        }

        public string GetId() => "fcuVsDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            while (value != 0)
            {
                simConnect.SendEvent(value < 0 ? dec : inc);
                value -= (short)Math.Sign(value);
            }
//TODO: in the real FCU, when turning quickly, it takes *two* clicks to change by 100 ft/min V/S.
        }
    }
}
