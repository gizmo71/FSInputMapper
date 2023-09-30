using Microsoft.Extensions.Logging;
using Lombok.NET;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class TaxiCam
    {
        private readonly ILogger<TaxiCam> _log;
    }
}
