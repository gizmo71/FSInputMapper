using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Com1Data
    {
        [SimVar("COM ACTIVE FREQUENCY:1", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 activeFrequency;
        [SimVar("COM STANDBY FREQUENCY:1", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standbyFrequency;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Com2Data
    {
        [SimVar("COM ACTIVE FREQUENCY:2", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 activeFrequency;
        [SimVar("COM STANDBY FREQUENCY:2", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standbyFrequency;
    };

    [Component]
    public class Com1RadioListener : DataListener<Com1Data>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public Com1RadioListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, Com1Data data)
        {
            hub.Clients.All.SetFrequencyFromSim("com1active", data.activeFrequency);
            hub.Clients.All.SetFrequencyFromSim("com1standby", data.standbyFrequency);
        }
    }

    [Component]
    public class Com2RadioListener : DataListener<Com2Data>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public Com2RadioListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, Com2Data data)
        {
            hub.Clients.All.SetFrequencyFromSim("com2active", data.activeFrequency);
            hub.Clients.All.SetFrequencyFromSim("com2standby", data.standbyFrequency);
        }
    }

    internal static class FrequencyExtensions
    {
        internal static void SetFrequencyFromSim(this IControlzmoHub hub, string field, int bcdHz)
        {
            string asString = String.Format("{0:X03}.{1:X03}", (bcdHz >> 16) & 0xFFF, (bcdHz >> 4) & 0xFFF);
            hub.SetFromSim(field, asString);
        }
    }
}