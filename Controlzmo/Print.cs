using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System.Drawing;
using System.Drawing.Printing;

// Also consider https://github.com/lukevp/ESC-POS-.NET
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
            _log.LogInformation("Print {}", value);

            var doc = new PrintDocument();
            doc.PrinterSettings.PrinterName = "Microsoft Print to PDF"; //TODO: POS58 Printer
            doc.PrintPage += (sender, e) => {
                //TODO: break longer lines up?
                e.Graphics?.DrawString(value, new Font("Arial", 8), Brushes.Black, 0, 0);
            };
            doc.Print();
        }
    }
}
