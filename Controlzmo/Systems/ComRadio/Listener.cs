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

        public ComRadioListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, ComData data)
        {
            hub.Clients.All.SetFromSim("com1active", FormatFrequency(data.com1ActiveFrequency));
            hub.Clients.All.SetFromSim("com1standby", FormatFrequency(data.com1StandbyFrequency));

            hub.Clients.All.SetFromSim("com2active", FormatFrequency(data.com2ActiveFrequency));
            hub.Clients.All.SetFromSim("com2standby", FormatFrequency(data.com2StandbyFrequency));

            hub.Clients.All.SetFromSim("com3active", FormatFrequency(data.com3ActiveFrequency));
            hub.Clients.All.SetFromSim("com3standby", FormatFrequency(data.com3StandbyFrequency));
        }

        private string FormatFrequency(int bcdHz) => String.Format("{0:X03}.{1:X03}", (bcdHz >> 16) & 0xFFF, (bcdHz >> 4) & 0xFFF);
    }
}