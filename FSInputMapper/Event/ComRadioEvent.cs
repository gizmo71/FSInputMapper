// Note that none of these affect what shows up on the radio panel in the A32NX. :-(
namespace FSInputMapper.Event
{

    [Singleton]
    public class Com1StandbyRadioSetEvent : IEvent
    {
        // In Hz, so 123.245 would be 123245000 - possibly in BCD (effectively hex)
        public string SimEvent() { return "COM_STBY_RADIO_SET"; }
    }
    //TODO: if we can't get that to work with 8.33kHz spacing, try COM_RADIO_WHOLE_INC/COM_RADIO_WHOLE_DEC and COM_RADIO_FRACT_INC/COM_RADIO_FRACT_DEC

    [Singleton]
    public class Com2StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() { return "COM2_STBY_RADIO_SET"; }
    }

    [Singleton]
    public class Com1StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() { return "COM_STBY_RADIO_SWAP"; }
    }

    [Singleton]
    public class Com2StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() { return "COM2_RADIO_SWAP"; }
    }

}
