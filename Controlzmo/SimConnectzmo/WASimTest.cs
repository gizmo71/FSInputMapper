using WASimCommander.CLI;
using WASimCommander.CLI.Enums;
using WASimCommander.CLI.Structs;
using WASimCommander.CLI.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Controlzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.FlightSimulator.SimConnect;

// https://github.com/mpaperno/WASimCommander/blob/main/src/Testing/CS_BasicConsole/Program.cs
namespace SimConnectzmo
{
    [Component]
    public class WasimTest : ISettable<string?>
    {
        private readonly ILogger _logger;
        
        public WasimTest(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<WasimTest>>();
        }

        public string GetId() => "testWasim";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
//TODO: can't load the DLL at runtime.
            var client = new WASimClient((uint)"Controlzmo".GetHashCode());

            // Monitor client state changes.
            client.OnClientEvent += ClientStatusHandler;
            // Subscribe to incoming log record events.
            client.OnLogRecordReceived += LogHandler;

            // As a test, set Client's callback logging level to display messages in the console.
            client.setLogLevel(WASimCommander.CLI.Enums.LogLevel.Info, LogFacility.Remote, LogSource.Client);
            // Set client's console log level to None to avoid double logging to our console. (Client also logs to a file by default.)
            client.setLogLevel(WASimCommander.CLI.Enums.LogLevel.None, LogFacility.Console, LogSource.Client);
            // Lets also see some log messages from the server
            client.setLogLevel(WASimCommander.CLI.Enums.LogLevel.Info, LogFacility.Remote, LogSource.Server);
        }

        private void ClientStatusHandler(ClientEvent clientEvent)
        {
            _logger.LogError($"Client event {clientEvent.eventType} - \"{clientEvent.message}\"; Client status: {clientEvent.status}", "^^");
        }

        void LogHandler(LogRecord lr, LogSource src)
        {
            _logger.LogError($"{src} Log: {lr}", "@@");  // LogRecord has a convenience ToString() override
        }
    }

    [Component]
    public class ToConfigTest : ISettable<string?>
    {
        private readonly JetBridgeSender jetbridge;

        public ToConfigTest(IServiceProvider serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "toConfig";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            jetbridge.Execute(simConnect, "1 (>L:A32NX_BTN_TOCONFIG)");
        }
    }
}