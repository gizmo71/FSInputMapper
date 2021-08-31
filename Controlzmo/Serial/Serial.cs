﻿using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace Controlzmo.Serial
{
    [Component]
    public class SerialPico : KeepAliveWorker, IOnSimStarted
    {
        private readonly ILogger _logger;
        private readonly SerialPort _serialPort;
        private readonly SimConnectHolder holder;
        private readonly IDictionary<string, ISettable> settables;

        public SerialPico(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<SerialPico>>();
            holder = serviceProvider.GetRequiredService<SimConnectHolder>();
            settables = serviceProvider
                .GetServices<ISettable>()
                .ToDictionary(settable => settable.GetId(), settable => settable);

            _serialPort = new SerialPort(portName: "COM3", baudRate: 115200, parity: Parity.None, dataBits: 8);
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.RequestToSend;
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 10000;
            _serialPort.WriteTimeout = 10000;
        }

        public void OnStarted(ExtendedSimConnect? _)
        {
            //TODO: do syncs in both directions only when both SimConnect and the Serial port are running and the sim is started.
            if (_serialPort.IsOpen)
            {
                var syncTimer = new System.Timers.Timer(5000);
                syncTimer.AutoReset = false;
                syncTimer.Elapsed += (object sender, ElapsedEventArgs args) => SendLine("SyncInputs");
                syncTimer.Start();
            }
        }

        protected override void OnStart(object? sender, DoWorkEventArgs args)
        {
//TODO: remove this interdependency
            if (!holder.SimConnect!.IsSimStared)
                throw new NullReferenceException("Aborting serial connection because SimConnect isn't attached with the sim running");

            _serialPort.Open();
            OnStarted(holder.SimConnect);
        }

        public void SendLine(string value)
        {
            if (!_serialPort.IsOpen)
            {
                _logger.LogWarning($"Cannot send '{value}' because serial port isn't open");
                return;
            }
_logger.LogInformation($"Sending '{value}'");
            byte[] data = Encoding.ASCII.GetBytes(value + "\n");
            _serialPort.Write(data, 0, data.Length);
        }

        private readonly Regex rx = new Regex(@"^([^=]+)=(.+)$", RegexOptions.Compiled);

        protected override void OnLoop(object? sender, DoWorkEventArgs args)
        {
            string message = ReadMessage();
            if (message.StartsWith('#'))
            {
                _logger.LogTrace($"{message}");
                return;
            }

            var match = rx.Match(message);
            if (!match.Success)
            {
                _logger.LogWarning($"Didn't recognise '{message}'");
                return;
            }

            var id = match.Groups[1].ToString();
            var value = match.Groups[2].ToString();
            Console.Error.WriteLine($"set {id} to {value}");

            ISettable? rawSettable;
            if (settables.TryGetValue(id, out rawSettable))
            {
                var typedValue = JsonSerializer.Deserialize(value, rawSettable.GetValueType());
                _logger.LogDebug($"Setting {id} to {typedValue}");
                rawSettable.SetInSim(holder.SimConnect!, typedValue);
            }
            else
                _logger.LogDebug($"Cannot find {id} to set it to {value}");
        }

        private string ReadMessage()
        {
            try
            {
                return _serialPort.ReadLine().Trim();
            }
            catch (TimeoutException)
            {
                return "# Nothing to read";
            }
        }

        protected override void OnStop(object? sender, DoWorkEventArgs args)
        {
            _serialPort.Close();
        }
    }
}
