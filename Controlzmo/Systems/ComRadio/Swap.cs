using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System.Transactions;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com1StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() => "COM1_RADIO_SWAP";
    }

    [Component]
    public class Com2StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() => "COM2_RADIO_SWAP";
    }

    [RequiredArgsConstructor]
    public abstract partial class ComSwap : ISettable<object>
    {
        private readonly JetBridgeSender sender;
        private readonly IEvent buttonEvent;
        private readonly int channel;

        public string GetId() => $"com{channel}swap";
        public void SetInSim(ExtendedSimConnect simConnect, object? source) {
            if (simConnect.IsFenix)
                for (int i = 0; i < 2; ++i)
                    sender.Execute(simConnect, $"(L:S_PED_RMP{channel}_XFER) ++ (>L:S_PED_RMP{channel}_XFER)");
            else
                simConnect.SendEvent(buttonEvent);
        }
    }

    [Component]
    public class Com1Swap : ComSwap
    {
        public Com1Swap(JetBridgeSender sender, Com1StandbyRadioSwapEvent swapEvent) : base(sender, swapEvent, 1) { }
    }

    [Component]
    public class Com2Swap : ComSwap
    {
        public Com2Swap(JetBridgeSender sender, Com2StandbyRadioSwapEvent swapEvent) : base(sender, swapEvent, 2) { }
    }
}
