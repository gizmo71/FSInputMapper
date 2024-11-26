using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;

namespace Controlzmo.Systems.Atc
{
    [Component]
    [RequiredArgsConstructor]
    public partial class AtcKey : ISettable<int>
    {
        private readonly JetBridgeSender sender;

        public string GetId() => "atcKey";

        public void SetInSim(ExtendedSimConnect simConnect, int value)
        {
            sender.Execute(simConnect, $"(>K:ATC_MENU_{value})");
        }
    }
}
