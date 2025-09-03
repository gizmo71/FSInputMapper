using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.Threading.Channels;

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
            if (simConnect.IsFenix)
            {
                var channel = GetId().Substring(3, 1).ToUpper();
                sender.Execute(simConnect, $"{newFrequency * 1000} (>L:N_PED_RMP{channel}_STDBY)");
            }
            else if (simConnect.IsIni321)
            {
                // HF doesn't really work at all!
                var channel = GetId().Substring(0, 4).ToUpper();
//TODO: might also want to set _STBY_FREQUENCY, which is just the two bits added together.
                sender.Execute(simConnect, $"{Decimal.Floor(newFrequency) * 1000} (>L:INI_{channel}_STBY_FREQUENCY_MHZ) {newFrequency % 1.0m * 1000} (>L:INI_{channel}_STBY_FREQUENCY_KHZ)");
                // But also fall through...
            }
            uint hz = Decimal.ToUInt32(Decimal.Multiply(newFrequency, new Decimal(1000000)));
            simConnect.SendEvent(setEvent, hz);
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
