using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com1StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM_STBY_RADIO_SET_HZ"; // and COM_RADIO_SET_HZ - or (BCD) COM_RADIO_SET and COM_STBY_RADIO_SET
    }

    [Component]
    public class Com2StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM2_STBY_RADIO_SET_HZ"; // and COM2_RADIO_SET_HZ - or (BCD) COM2_RADIO_SET and COM2_STBY_RADIO_SET
    }

    [RequiredArgsConstructor]
    public abstract partial class AbstractComStandby : ISettable<string?>
    {
        private readonly IEvent setEvent;
        private readonly JetBridgeSender sender;

        public abstract string GetId();

        public void SetInSim(ExtendedSimConnect simConnect, string? newFrequencyString)
        {
            var newFrequency = Decimal.Parse(newFrequencyString!);
            uint hz = Decimal.ToUInt32(Decimal.Multiply(newFrequency, new Decimal(1000000)));
            simConnect.SendEvent(setEvent, hz, 0, SimConnect.SIMCONNECT_GROUP_PRIORITY_DEFAULT);
        }
    }

    [Component]
    public class Com1Standby : AbstractComStandby
    {
        public Com1Standby(Com1StandbyRadioSetEvent set, JetBridgeSender sender) : base(set, sender) { }
        public override string GetId() => "com1standby";
    }

    [Component]
    public class Com2Standby : AbstractComStandby
    {
        public Com2Standby(Com2StandbyRadioSetEvent set, JetBridgeSender sender) : base(set, sender) { }
        public override string GetId() => "com2standby";
    }
}
