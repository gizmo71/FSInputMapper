using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text.RegularExpressions;

// Also consider https://github.com/lukevp/ESC-POS-.NET
namespace Controlzmo
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Print : ISettable<string>
    {
        private static readonly Font MM58_FONT = new Font("Consolas", 9.7f);
        private static readonly int COLS = 24;

        private readonly ILogger<Print> _log;

        public string GetId() => "printText";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            _log.LogInformation("Print {}", value);
            if (value == null || value.Trim() == "") return;
            value = String.Join("\n", WordWrap(value));

            var doc = new PrintDocument();
            doc.PrinterSettings.PrinterName = "POS58 Printer"; // or "Microsoft Print to PDF"
            doc.PrintPage += (sender, e) => {
                e.Graphics?.DrawString(value, MM58_FONT, Brushes.Black, 0, 0);
            };
            doc.Print();
        }

        private static IEnumerable<String> WordWrap(string source)
        {
            // Based on https://stackoverflow.com/a/41287371
            return new Regex(
                @"(?:[^\r\n]{1,$LL$}(?=\s|$)|[^\r\n]{$LL$}|(?<=\n)\r?\n)"
                    .Replace("$LL$", COLS.ToString()))
                .Matches(source)
                .Cast<Match>()
                .Select(m => m.Value.Trim());
        }
    }
}
