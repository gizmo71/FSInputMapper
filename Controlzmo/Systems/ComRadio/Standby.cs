using System;
using Controlzmo.Hubs;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com1StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM_STBY_RADIO_SET_HZ";
    }

    [Component]
    public class Com2StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM2_STBY_RADIO_SET_HZ";
    }

    public abstract class AbstractComStandby : ISettable<string?>
    {
        private readonly IEvent setEvent;

        protected AbstractComStandby(IEvent setEvent)
        {
            this.setEvent = setEvent;
        }

        public abstract string GetId();

        public void SetInSim(ExtendedSimConnect simConnect, string? newFrequencyString)
        {
            var newFrequency = Decimal.Parse(newFrequencyString!);
            uint hz = Decimal.ToUInt32(Decimal.Multiply(newFrequency, new Decimal(1000000)));
            simConnect.SendEvent(setEvent, hz);
        }
    }

    [Component]
    public class Com1Standby : AbstractComStandby
    {
        public Com1Standby(Com1StandbyRadioSetEvent set) : base(set) { }
        public override string GetId() => "com1standby";
    }

    [Component]
    public class Com2Standby : AbstractComStandby
    {
        public Com2Standby(Com2StandbyRadioSetEvent set) : base(set) { }
        public override string GetId() => "com2standby";
    }
}
