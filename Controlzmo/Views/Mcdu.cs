using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using SimConnectzmo;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Mcdu : IOnSimStarted, ISettable<object>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        public void OnStarted(ExtendedSimConnect simConnect)
        {
            var type = simConnect.IsFenix ? "fenix" : "fbw";
            hub.Clients.All.SetMcduType(type);
        }
 
        public string GetId() => "resetMcdu";

        public void SetInSim(ExtendedSimConnect simConnect, object? value)
        {
            OnStarted(simConnect);
        }

    }
}
