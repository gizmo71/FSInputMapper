using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com1StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() => "COM_STBY_RADIO_SWAP";
    }

    [Component]
    public class Com2StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() => "COM2_RADIO_SWAP";
    }

    [Component]
    public class Com3StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() => "COM3_RADIO_SWAP";
    }

    [Component]
    public class Com1Swap : AbstractButton
    {
        public Com1Swap(Com1StandbyRadioSwapEvent swapEvent) : base(swapEvent) { }
        public override string GetId() => "com1swap";
    }

    [Component]
    public class Com2Swap : AbstractButton
    {
        public Com2Swap(Com2StandbyRadioSwapEvent swapEvent) : base(swapEvent) { }
        public override string GetId() => "com2swap";
    }

    [Component]
    public class Com3Swap : AbstractButton
    {
        public Com3Swap(Com3StandbyRadioSwapEvent swapEvent) : base(swapEvent) { }
        public override string GetId() => "com3swap";
    }
}
