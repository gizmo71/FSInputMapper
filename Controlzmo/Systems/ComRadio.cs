using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems
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

    [Component]
    public class Com1StandbyRadioSetEvent : IEvent
    {
        // In Hz, so 123.245 would be 123245000 - possibly in BCD (effectively hex)
        public string SimEvent() => "COM_STBY_RADIO_SET";
//TODO: if we can't get that to work with 8.33kHz spacing, try COM_RADIO_WHOLE_INC/COM_RADIO_WHOLE_DEC and COM_RADIO_FRACT_INC/COM_RADIO_FRACT_DEC
    }

    [Component]
    public class Com2StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM2_STBY_RADIO_SET";
    }

    [Component]
    public class Com3StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM3_STBY_RADIO_SET";
    }

    [Component]
    public class Com1StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() => "COM_STBY_RADIO_SWAP";
    }

    [Component]
    public class Com2StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() => "COM2_RADIO_SWAP";
    }

    [Component]
    public class Com3StandbyRadioSwapEvent : IEvent
    {
        public string SimEvent() => "COM3_RADIO_SWAP";
    }

#if false
    [Component]
    public class ComRadioSystem
    {
        private readonly Com1StandbyRadioSetEvent com1StandbyRadioSetEvent;
        private readonly Com1StandbyRadioSwapEvent com1StandbyRadioSwapEvent;
        private readonly SimConnectHolder sch;

        public ComRadioSystem(Com1StandbyRadioSetEvent com1StandbyRadioSetEvent, Com1StandbyRadioSwapEvent com1StandbyRadioSwapEvent, SimConnectHolder sch)
        {
            this.com1StandbyRadioSetEvent = com1StandbyRadioSetEvent;
            this.com1StandbyRadioSwapEvent = com1StandbyRadioSwapEvent;
            this.sch = sch;
        }

        public string SetCom1Standby(Decimal newFrequency)
        {
            uint kHz = Decimal.ToUInt32(Decimal.Multiply(newFrequency, new Decimal(1000)));
            string bcdHex = kHz.ToString("D6");
            // Currently, MSFS doesn't support setting the first or last digit directly. :-(
            bcdHex = bcdHex.Substring(1, 4);
//throw new Exception($"hex {bcdHex}");
            uint bcd = uint.Parse(bcdHex, System.Globalization.NumberStyles.HexNumber);
            sch.SimConnect?.SendEvent(com1StandbyRadioSetEvent, bcd);
            return bcdHex;
        }

        public void SwapCom1()
        {
            sch.SimConnect?.SendEvent(com1StandbyRadioSwapEvent);
        }
    }
#endif
}
