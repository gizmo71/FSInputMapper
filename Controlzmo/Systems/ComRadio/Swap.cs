using Controlzmo.Hubs;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [RequiredArgsConstructor]
    public abstract partial class ComSwap : ISettable<object>
    {
        private readonly IComRadio radio;

        public string GetId() => $"com{radio.Channel}swap";
        public void SetInSim(ExtendedSimConnect simConnect, object? source) {
            radio.Swap(simConnect);
        }
    }

    [Component]
    public class Com1Swap : ComSwap
    {
        public Com1Swap(Com1RadioListener radio) : base(radio) { }
    }

    [Component]
    public class Com2Swap : ComSwap
    {
        public Com2Swap(Com2RadioListener radio) : base(radio) { }
    }
}
