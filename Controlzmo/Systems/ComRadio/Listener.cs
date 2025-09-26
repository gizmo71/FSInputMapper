using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.ComRadio
{
    public interface IComRadioData
    {
        public UInt32 ActiveHz { get; }
        public UInt32 StandbyHz { get; }
    }

    [RequiredArgsConstructor]
    public abstract partial class ComRadioManager<T> : DataListener<T>, IRequestDataOnOpen where T : struct, IComRadioData
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly int channel;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, T data)
        {
            hub.Clients.All.SetFrequencyFromSim($"com{channel}active", data.ActiveHz);
            hub.Clients.All.SetFrequencyFromSim($"com{channel}standby", data.StandbyHz);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct Com1Data : IComRadioData
    {
        [Property]
        [SimVar("COM ACTIVE FREQUENCY:1", "Hertz", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public UInt32 _activeHz;
        [Property]
        [SimVar("COM STANDBY FREQUENCY:1", "Hertz", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public UInt32 _standbyHz;
    };

    [Component]
    public class Com1RadioListener : ComRadioManager<Com1Data>
    {
        public Com1RadioListener(IHubContext<ControlzmoHub, IControlzmoHub> hub) : base(hub, 1) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct Com2Data : IComRadioData
    {
        [Property]
        [SimVar("COM ACTIVE FREQUENCY:2", "Hertz", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public UInt32 _activeHz;
        [Property]
        [SimVar("COM STANDBY FREQUENCY:2", "Hertz", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public UInt32 _standbyHz;
    };

    [Component]
    public class Com2RadioListener : ComRadioManager<Com2Data>
    {
        public Com2RadioListener(IHubContext<ControlzmoHub, IControlzmoHub> hub) : base(hub, 2) { }
    }

    internal static class FrequencyExtensions
    {
        internal static void SetFrequencyFromSim(this IControlzmoHub hub, string field, uint hertz)
        {
            var mhz = Decimal.Divide(hertz, 1_000_000);
            string asString = String.Format("{0:000.000}", mhz);
            hub.SetFromSim(field, asString);
        }
    }
}