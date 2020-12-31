using System;
using System.Runtime.InteropServices;
using FSInputMapper.Data;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Altimeter
{

    [Singleton]
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

        public void SetCom1Standby(Decimal newFrequency)
        {
            uint kHz = Decimal.ToUInt32(Decimal.Multiply(newFrequency, new Decimal(1000)));
            string hex = kHz.ToString("x8");
            uint bcd = uint.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            sch.SimConnect?.SendEvent(com1StandbyRadioSetEvent, bcd);
        }

        public void SwapCom1()
        {
            sch.SimConnect?.SendEvent(com1StandbyRadioSwapEvent);
        }

    }

}
