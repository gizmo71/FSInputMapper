using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.ComRadio
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Com1Data
    {
        [SimVar("COM ACTIVE FREQUENCY:1", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 activeFrequency;
        [SimVar("COM STANDBY FREQUENCY:1", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standbyFrequency;
        [SimVar("L:INI_COM1_STBY_FREQUENCY", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 iniStandby;
        [SimVar("L:N_PED_RMP1_STDBY", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenixStandby;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Com2Data
    {
        [SimVar("COM ACTIVE FREQUENCY:2", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 activeFrequency;
        [SimVar("COM STANDBY FREQUENCY:2", "Frequency BCD32", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standbyFrequency;
        [SimVar("L:INI_COM2_STBY_FREQUENCY", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 iniStandby;
        [SimVar("L:N_PED_RMP2_STDBY", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenixStandby;
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
            if (simConnect.IsFenix)
                data.standbyFrequency = FrequencyExtensions.fromIni(data.fenixStandby);
            else if (simConnect.IsIni321)
                data.standbyFrequency = FrequencyExtensions.fromIni(data.iniStandby);
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
            if (simConnect.IsFenix)
                data.standbyFrequency = FrequencyExtensions.fromIni(data.fenixStandby);
            else if (simConnect.IsIni321)
                data.standbyFrequency = FrequencyExtensions.fromIni(data.iniStandby);
            hub.Clients.All.SetFrequencyFromSim("com2active", data.activeFrequency);
            hub.Clients.All.SetFrequencyFromSim("com2standby", data.standbyFrequency);
        }
    }

    internal static class FrequencyExtensions
    {
        internal static void SetFrequencyFromSim(this IControlzmoHub hub, string field, int bcdHz)
        {
            // For example, 121.500 is 0x01215000.
            string asString = String.Format("{0:X03}.{1:X03}", (bcdHz >> 16) & 0xFFF, (bcdHz >> 4) & 0xFFF);
            hub.SetFromSim(field, asString);
        }

        internal static Int32 fromIni(Int32 ini) {
            try
            {
                return Convert.ToInt32(String.Format("0x{0}", ini * 10), 16);
            }
            catch (Exception t)
            {
                Console.Error.WriteLine(t);
                return 0x6666660;
            }
        }
    }
}