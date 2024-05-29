using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Controlzmo.Systems.Atc
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FetchMetar : ISettable<string>
    {
        private readonly static Regex icaoRegex = new Regex(@"^[A-Z]{4}$");

        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger<FetchMetar> _log;

        public string GetId() => "fetchMetar";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            if (value != null && icaoRegex.IsMatch(value))
            {
                // https://aviationweather.gov/data/api/#cache e.g. https://aviationweather.gov/api/data/metar?ids=EGMC&format=raw&taf=false
                _log.LogInformation("Fetch METAR for {}", value);
                fetchMetar(value);
            }
            else
                _log.LogCritical("Not a valid ICAO for metar '{}'", value);
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
