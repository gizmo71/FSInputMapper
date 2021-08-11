using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
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
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
        }

        protected override void OnStart(object? sender, DoWorkEventArgs args)
        {
            _serialPort.Open();
        }

        private byte[] writeData = { 0 };

        protected override void OnLoop(object? sender, DoWorkEventArgs args)
        {
            try
            {
                string message = _serialPort.ReadLine();
                Console.Error.WriteLine(message);
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
