using System;
using System.Text.RegularExpressions;
using Controlzmo.Hubs;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com1StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM_STBY_RADIO_SET";
    }

    [Component]
    public class Com1StandbyRadioBumpEvent : IEvent
    {
        public string SimEvent() => "COM_RADIO_FRACT_INC";
    }

    [Component]
    public class Com2StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM2_STBY_RADIO_SET";
    }

    [Component]
    public class Com2StandbyRadioBumpEvent : IEvent
    {
        public string SimEvent() => "COM2_RADIO_FRACT_INC";
    }

    [Component]
    public class Com3StandbyRadioSetEvent : IEvent
    {
        public string SimEvent() => "COM3_STBY_RADIO_SET";
    }

    [Component]
    public class Com3StandbyRadioBumpEvent : IEvent
    {
        public string SimEvent() => "COM3_RADIO_FRACT_INC";
    }

    public abstract class AbstractComStandby : ISettable<string?>
    {
        // In Hz, so 123.245 would be 123245000 - possibly in BCD (effectively hex)
        // Currently, MSFS doesn't support setting the first or last digit directly. :-(
        private readonly IEvent setEvent;
        private readonly IEvent bumpEvent;

        protected AbstractComStandby(IEvent setEvent, IEvent bumpEvent)
        {
            this.setEvent = setEvent;
            this.bumpEvent = bumpEvent;
        }

        public abstract string GetId();

        // https://www.caa.co.uk/General-aviation/Aircraft-ownership-and-maintenance/8-33-kHz-radios/
        // 00, 05, 10, 15, 25, 30, 35, 40, 50, 55, 60, 65, 75, 80. 85, 90
        private static readonly Regex NeedsBump = new Regex(@"[013568]5$", RegexOptions.Compiled);

        public void SetInSim(ExtendedSimConnect simConnect, string? newFrequencyString)
        {
            var newFrequency = Decimal.Parse(newFrequencyString!);
            uint kHz = Decimal.ToUInt32(Decimal.Multiply(newFrequency, new Decimal(1000)));
            string bcdHex = kHz.ToString("D6");
            uint bcdCentre = uint.Parse(bcdHex.Substring(1, 4), System.Globalization.NumberStyles.HexNumber);
            simConnect.SendEvent(setEvent, bcdCentre);
            if (NeedsBump.IsMatch(bcdHex)) {
                simConnect.SendEvent(bumpEvent);
            }
        }
    }

    [Component]
    public class Com1Standby : AbstractComStandby
    {
        public Com1Standby(Com1StandbyRadioSetEvent set, Com1StandbyRadioBumpEvent bump) : base(set, bump) { }
        public override string GetId() => "com1standby";
    }

    [Component]
    public class Com2Standby : AbstractComStandby
    {
        public Com2Standby(Com2StandbyRadioSetEvent set, Com2StandbyRadioBumpEvent bump) : base(set, bump) { }
        public override string GetId() => "com2standby";
    }

    [Component]
    public class Com3Standby : AbstractComStandby
    {
        public Com3Standby(Com3StandbyRadioSetEvent set, Com3StandbyRadioBumpEvent bump) : base(set, bump) { }
        public override string GetId() => "com3standby";
    }
}
