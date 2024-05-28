using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SimConnectzmo;

namespace Controlzmo.Systems.Atc
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FetchMetar : ISettable<string>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger<FetchMetar> _log;

        public string GetId() => "fetchMetar";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            _log.LogCritical("Fetch METAR for {}", value);
            hub.Clients.All.SetFromSim("metar", $"TODO: METAR for {value}");
        }
    }
}
