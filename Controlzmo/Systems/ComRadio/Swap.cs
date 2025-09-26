using Controlzmo.Hubs;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [RequiredArgsConstructor]
    public abstract partial class ComSwap : ISettable<object>
    {
        private readonly int channel;
        private readonly IEvent _event;

        public string GetId() => $"com{channel}swap";
        public void SetInSim(ExtendedSimConnect simConnect, object? source) {
            simConnect.SendEvent(_event, 0, 0, ExtendedSimConnect.SIMCONNECT_GROUP_PRIORITY_STANDARD);
        }
    }

    [Component]
    public class Com1StandbyRadioSwapEvent : IEvent { public string SimEvent() => "COM1_RADIO_SWAP"; }

    [Component]
    public class Com1Swap : ComSwap
    {
        public Com1Swap(Com1StandbyRadioSwapEvent e) : base(1, e) { }
    }


    [Component]
    public class Com2StandbyRadioSwapEvent : IEvent { public string SimEvent() => "COM2_RADIO_SWAP"; }

    [Component]
    public class Com2Swap : ComSwap
    {
        public Com2Swap(Com2StandbyRadioSwapEvent e) : base(2, e) { }
    }
}
