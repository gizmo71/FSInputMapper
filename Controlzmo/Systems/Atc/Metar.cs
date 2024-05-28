using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System.Net.Http;

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
            //TODO: validate "value" properly
            if (value != null)
            {
                // https://aviationweather.gov/data/api/#cache e.g. https://aviationweather.gov/api/data/metar?ids=EGMC&format=raw&taf=false
                _log.LogCritical("Fetch METAR for {}", value);
                fetchMetar(value);
            }
        }

        private async void fetchMetar(string icao)
        {
            using (var client = new HttpClient())
            {
                string s = await client.GetStringAsync($"https://aviationweather.gov/api/data/metar?ids={icao}&format=raw&taf=false");
                await hub.Clients.All.SetFromSim("metar", s);
            }
        }
    }
}
