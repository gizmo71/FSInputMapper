using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;
using System.IO;

namespace Controlzmo
{
    [Component]
    [RequiredArgsConstructor]
    public partial class Print : ISettable<string>, IDisposable
    {
        private readonly ILogger<Print> _log;
        private string? file;

        public string GetId() => "printText";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            _log.LogInformation("Print {}", value);
            if (file == null)
            {
                file = Path.GetTempFileName() + ".Controlzmo.METAR.txt";
            }

            // https://stackoverflow.com/questions/70780652/how-to-print-a-text-file-content-in-net-using-c-sharp
            using (StreamWriter outputFile = new StreamWriter(file))
            {
                outputFile.WriteLine(value);
                outputFile.WriteLine();
                outputFile.WriteLine(new String('-', 15));
            }
            using (var p = new System.Diagnostics.Process()) {
                p.StartInfo.FileName = "notepad";
                p.StartInfo.Arguments = $"/pt \"{file}\" \"POS58 Printer\"";
                p.StartInfo.UseShellExecute = false;
                p.Start();
            }
        }

        public void Dispose()
        {
            if (file != null)
                File.Delete(file);
        }
    }
}
