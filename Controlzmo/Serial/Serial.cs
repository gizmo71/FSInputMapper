using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Controlzmo.Hubs;
using SimConnectzmo;
using System.IO.Ports;
using System.Threading;

namespace Controlzmo.Serial
{
    [Component]
    public class Serial : CreateOnStartup, IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly Thread readThread; // Convert to BackgroundWorker a la Adapter
        private bool running = true;

        public Serial()
        {
            _serialPort = new SerialPort(portName: "COM3", baudRate: 115200, parity: Parity.None, dataBits: 8);
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.RequestToSend;
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Open();
            //TOOD: read thread... https://docs.microsoft.com/en-us/dotnet/api/system.io.ports.serialport?view=dotnet-plat-ext-5.0
            readThread = new Thread(Read);
            readThread.IsBackground = true;
            readThread.Start();
            Console.Error.WriteLine("Started serial <><><><><>");
        }

        public void Read()
        {
            byte[] writeData = { 0 };
            while (running)
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
        }

        public void Dispose()
        {
            running = false;
            readThread.Join();
            _serialPort.Close();
        }
    }
}
