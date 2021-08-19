using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Controlzmo.Hubs;
using Controlzmo.Systems.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimConnectzmo;

namespace Controlzmo.Serial
{
    [Component]
    public class Serial : KeepAliveWorker, CreateOnStartup
    {
        private readonly ILogger _logger;
        private readonly SerialPort _serialPort;
        private readonly RunwayTurnoffLightSystem lights;
        private readonly SimConnectHolder holder;
        private readonly IDictionary<string, ISettable> settables;

        public Serial(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<Serial>>();
            holder = serviceProvider.GetRequiredService<SimConnectHolder>();
            settables = serviceProvider
                .GetServices<ISettable>()
                .ToDictionary(settable => settable.GetId(), settable => settable);

            lights = serviceProvider.GetRequiredService<RunwayTurnoffLightSystem>();

            _serialPort = new SerialPort(portName: "COM3", baudRate: 115200, parity: Parity.None, dataBits: 8);
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.RequestToSend;
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 10000;
            _serialPort.WriteTimeout = 10000;
        }

        protected override void OnStart(object? sender, DoWorkEventArgs args)
        {
            if (holder.SimConnect == null)
                throw new NullReferenceException("Aborting serial connection because SimConnect isn't attached");

            _serialPort.Open();

            byte[] writeData = { 1 };
            _serialPort.Write(writeData, 0, 1);
        }

        private readonly Regex rx = new Regex(@"^([^=]+)=(.+)$", RegexOptions.Compiled);

        protected override void OnLoop(object? sender, DoWorkEventArgs args)
        {
            ExtendedSimConnect simConnect = holder.SimConnect!;
            try
            {
                string message = _serialPort.ReadLine().Trim();
                var match = rx.Match(message);
                if (!match.Success)
                    throw new Exception($"Didn't recognise '{message}'");
                var id = match.Groups[1].ToString();
                var value = match.Groups[2].ToString();
                Console.Error.WriteLine($"set {id} to {value}");

                ISettable rawSettable = settables[id];
                var typedValue = JsonSerializer.Deserialize(value, rawSettable.GetValueType());
                _logger.LogDebug($"Setting {id} to {typedValue}");
                rawSettable.SetInSim(simConnect, typedValue);
            }
            catch (TimeoutException)
            {
                _logger.LogDebug("Nothing to read");
            }
        }

        protected override void OnStop(object? sender, DoWorkEventArgs args)
        {
            _serialPort.Close();
        }
    }
}
