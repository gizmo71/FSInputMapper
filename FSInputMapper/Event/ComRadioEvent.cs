using FSInputMapper.Data;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Event
{

    [Singleton]
    public class Com1StandbyRadioSetEvent : IEvent
    {
        // In Hz, so 123.245 would be 123245000 - possibly in BCD (effectively hex)
        public string SimEvent() { return "COM_STBY_RADIO_SET"; }
    }

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
        public string SimEvent() { return "COM_STBY_RADIO_SWAP"; }
    }

}
