using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Controlzmo.Serial
{
    [Component]
    public class Serial : KeepAliveWorker, CreateOnStartup
    {
        private readonly ILogger _logger;
        private readonly SerialPort _serialPort;

        public Serial(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<Serial>>();

            _serialPort = new SerialPort(portName: "COM3", baudRate: 115200, parity: Parity.None, dataBits: 8);
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.RequestToSend;
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
        }

        protected override void OnStart(object? sender, DoWorkEventArgs args)
        {
            _serialPort.Open();
        }

        private readonly Regex rx = new Regex(@"^(\d+), ([01])/([01])$", RegexOptions.Compiled);
        private readonly byte[] writeData = { 0 };

        protected override void OnLoop(object? sender, DoWorkEventArgs args)
        {
            try
            {
                string message = _serialPort.ReadLine().Trim();
                var match = rx.Match(message);
                if (!match.Success)
                    throw new Exception($"Didn't recognise '{message}'");
                var pot = Int16.Parse(match.Groups[1].ToString());
                var s1 = Int16.Parse(match.Groups[2].ToString());
                var s2 = Int16.Parse(match.Groups[3].ToString());
                Console.Error.WriteLine($"Pot is {pot}, switches are {s1}/{s2}");
            }
            catch (TimeoutException)
            {
                Console.Error.WriteLine("Nothing to read");
                writeData[0] ^= 1;
                _serialPort.Write(writeData, 0, 1);
            }
        }

        protected override void OnStop(object? sender, DoWorkEventArgs args)
        {
            _serialPort.Close();
        }
    }
}
