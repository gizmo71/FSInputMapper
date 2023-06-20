using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuSpeedMachToggled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_MACH_TOGGLE_PUSH";
        public string GetId() => "speedMachToggled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuSpeedPulled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_PULL";
        public string GetId() => "fcuSpeedPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuSpeedPushed : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_PUSH";
        public string GetId() => "fcuSpeedPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuSpeedInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_INC";
    }

    [Component]
    public class FcuSpeedDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_DEC";
    }

    [Component]
    public class FcuSpeedDelta : ISettable<Int16>
    {
        private readonly FcuSpeedInc inc;
        private readonly FcuSpeedDec dec;

        public FcuSpeedDelta(IServiceProvider sp)
        {
            inc = sp.GetRequiredService<FcuSpeedInc>();
            dec = sp.GetRequiredService<FcuSpeedDec>();
        }

        public string GetId() => "fcuSpeedDelta";

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
