using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com1RxToggleEvent : IEvent
    {
        public string SimEvent() => "COM1_RECEIVE_SELECT";
    }

    [Component]
    public class Com2RxToggleEvent : IEvent
    {
        public string SimEvent() => "COM2_RECEIVE_SELECT";
    }

    [Component]
    public class Com3RxToggleEvent : IEvent
    {
        public string SimEvent() => "COM3_RECEIVE_SELECT";
    }

    [Component]
    public class Com1RxToggle : AbstractButton
    {
        public Com1RxToggle(Com1RxToggleEvent toggleEvent) : base(toggleEvent) { }
        public override string GetId() => "com1rxToggle";
    }

    [Component]
    public class Com2RxToggle : AbstractButton
    {
        public Com2RxToggle(Com2RxToggleEvent toggleEvent) : base(toggleEvent) { }
        public override string GetId() => "com2rxToggle";
    }

    [Component]
    public class Com3RxToggle : AbstractButton
    {
        public Com3RxToggle(Com3RxToggleEvent toggleEvent) : base(toggleEvent) { }
        public override string GetId() => "com3rxToggle";
    }
}
