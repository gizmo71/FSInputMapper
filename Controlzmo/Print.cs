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
        private readonly Printer printer;

        public string GetId() => "printText";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            printer.Print(value ?? "", 32); // from Fenix
        }
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class Printer
    {
        //private readonly ILogger<Printer> _log;

        public void Print(string value, int columns)
        {
            if (value.Trim() == "") return;
            value = String.Join("\n", WordWrap(value, columns));

            var doc = new PrintDocument();
            doc.PrinterSettings.PrinterName = "POS58 Printer"; // or "Microsoft Print to PDF"
            doc.PrintPage += (sender, printEvent) => {
                var mmCharWidth = 48f / columns;
                var font = new Font("Arial", mmCharWidth * 1.7f, GraphicsUnit.Millimeter);
                    printEvent.Graphics?.DrawString(value, font, Brushes.Black, 0, 0);
            };
            doc.Print();
        }

        private static IEnumerable<String> WordWrap(string source, int columns)
        {
            // Based on https://stackoverflow.com/a/41287371
            return new Regex(
                @"(?:[^\r\n]{1,$LL$}(?!\xA0)(?=\s|$)|[^\r\n]{$LL$}|(?<=\n)\r?\n)"
                    .Replace("$LL$", columns.ToString()))
                .Matches(source)
                .Cast<Match>()
                .Select(m => m.Value.Trim());
        }
    }
}
