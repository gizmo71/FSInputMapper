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

    public interface IComRadio
    {
        public void Swap(ExtendedSimConnect simConnect);
        public int Channel { get; }
    }

    [RequiredArgsConstructor]
    public abstract partial class ComRadioManager<T> : DataListener<T>, IComRadio, IRequestDataOnOpen where T : struct, IComRadioData
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly IEvent setActive;
        private readonly IEvent setStandby;
        [Property]
        private readonly int _channel;
        private IComRadioData current;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, T data)
        {
            current = data;
            hub.Clients.All.SetFrequencyFromSim($"com{Channel}active", data.ActiveHz);
            hub.Clients.All.SetFrequencyFromSim($"com{Channel}standby", data.StandbyHz);
        }

        public void Swap(ExtendedSimConnect sc)
        {
Console.Error.WriteLine($"swapping {current.ActiveHz} and {current.StandbyHz}");
            var _new = current;
//TODO: this just doesn't work in the Fenix - probably we can't genuinely bypass whatever its doing to steal the values. :-(
            sc.SendEvent(setActive, _new.StandbyHz, 0, SimConnect.SIMCONNECT_GROUP_PRIORITY_DEFAULT);
            sc.SendEvent(setStandby, _new.ActiveHz, 0, SimConnect.SIMCONNECT_GROUP_PRIORITY_DEFAULT);
        }
    }

    [Component]
    public class Com1ActiveSetEvent : IEvent { public string SimEvent() => "COM_RADIO_SET_HZ"; }

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
        public Com1RadioListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, Com1ActiveSetEvent a, Com1StandbyRadioSetEvent s) : base(hub, a, s, 1) { }
    }

    [Component]
    public class Com2ActiveSetEvent : IEvent { public string SimEvent() => "COM2_RADIO_SET_HZ"; }

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
        public Com2RadioListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, Com2ActiveSetEvent a, Com2StandbyRadioSetEvent s) : base(hub, a, s, 2) { }
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