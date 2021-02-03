using System;
using Controlzmo.Hubs;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com1StandbyRadioSetEvent : IEvent
    {
        // In Hz, so 123.245 would be 123245000 - possibly in BCD (effectively hex)
        public string SimEvent() => "COM_STBY_RADIO_SET";
    }

    [Component]
    public class Com2StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM2_STBY_RADIO_SET";
    }

    [Component]
    public class Com3StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM3_STBY_RADIO_SET";
    }

    public abstract class AbstractComStandby : ISettable<string?>
    {
        private readonly IEvent setEvent;

        protected AbstractComStandby(IEvent setEvent) => this.setEvent = setEvent;

        public abstract string GetId();

        public void SetInSim(ExtendedSimConnect simConnect, string? newFrequencyString)
        {
            var newFrequency = Decimal.Parse(newFrequencyString!);
            uint kHz = Decimal.ToUInt32(Decimal.Multiply(newFrequency, new Decimal(1000)));
            string bcdHex = kHz.ToString("D6");
            // Currently, MSFS doesn't support setting the first or last digit directly. :-(
            //TODO: if we can't get this to work with 8.33kHz spacing, try COM_RADIO_WHOLE_INC/COM_RADIO_WHOLE_DEC and COM_RADIO_FRACT_INC/COM_RADIO_FRACT_DEC
            bcdHex = bcdHex.Substring(1, 4);
            //throw new Exception($"hex {bcdHex}");
            uint bcd = uint.Parse(bcdHex, System.Globalization.NumberStyles.HexNumber);
            simConnect.SendEvent(setEvent, bcd);
            //return bcdHex;
        }
    }

    [Component]
    public class Com1Standby : AbstractComStandby
    {
        public Com1Standby(Com1StandbyRadioSetEvent setEvent) : base(setEvent) { }
        public override string GetId() => "com1standby";
    }

    [Component]
    public class Com2Standby : AbstractComStandby
    {
        public Com2Standby(Com2StandbyRadioSetEvent setEvent) : base(setEvent) { }
        public override string GetId() => "com2standby";
    }

    [Component]
    public class Com3Standby : AbstractComStandby
    {
        public Com3Standby(Com3StandbyRadioSetEvent setEvent) : base(setEvent) { }
        public override string GetId() => "com3standby";
    }
}
