using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ComData
    {
        [SimVar("COM ACTIVE FREQUENCY:1", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 com1ActiveFrequency;
        [SimVar("COM STANDBY FREQUENCY:1", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 com1StandbyFrequency;
        [SimVar("COM ACTIVE FREQUENCY:2", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 com2ActiveFrequency;
        [SimVar("COM STANDBY FREQUENCY:2", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 com2StandbyFrequency;
        [SimVar("COM ACTIVE FREQUENCY:3", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 com3ActiveFrequency;
        [SimVar("COM STANDBY FREQUENCY:3", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 com3StandbyFrequency;
    };

    [Component]
    public class ComRadioListener : DataListener<ComData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger _logger;

        public ComRadioListener(ILogger<ComRadioListener> _logger, IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this._logger = _logger;
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, ComData data)
        {
            var com1 = String.Format("COM1: Active {0:X06}\nStandby {1:X06}", data.com1ActiveFrequency >> 4, data.com1StandbyFrequency >> 4);
            var com2 = String.Format("COM2: Active {0:X06}\nStandby {1:X06}", data.com2ActiveFrequency >> 4, data.com2StandbyFrequency >> 4);
            var com3 = String.Format("COM3: Active {0:X06}\nStandby {1:X06}", data.com3ActiveFrequency >> 4, data.com3StandbyFrequency >> 4);
            _logger.LogDebug($"{com1}\n{com2}\n{com3}");
        }
    }
}