using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;

namespace Controlzmo
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Print : ISettable<string>
    {
        private readonly ILogger<Print> _log;

        public string GetId() => "printText";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            _log.LogCritical("TODO: print {}", value);
        }
    }
}
