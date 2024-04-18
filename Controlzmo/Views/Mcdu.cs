using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using SimConnectzmo;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Mcdu : IOnSimStarted
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        public void OnStarted(ExtendedSimConnect simConnect)
        {
            var type = simConnect.IsFenix ? "fenix" : "fbw";
            hub.Clients.All.SetMcduType(type);
        }
    }
}
