using System;
using System.Runtime.InteropServices;
using FSInputMapper.Data;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Systems.Apu
{

    [Singleton]
    public class ApuToggle : IEvent { public string SimEvent() { return "APU_GENERATOR_SWITCH_TOGGLE"; } }

    [Singleton]
    public class ApuStart : IEvent { public string SimEvent() { return "APU_STARTER"; } }

    [Singleton]
    public class ApuSystem
    {
        private readonly ApuToggle apuToggle;
        private readonly ApuStart apuStart;
        private readonly SimConnectHolder sch;

        public ApuSystem(ApuToggle apuToggle, ApuStart apuStart, SimConnectHolder sch)
        {
            this.apuToggle = apuToggle;
            this.apuStart = apuStart;
            this.sch = sch;
        }

        public void ApuToggle()
        {
            //sch.SimConnect?.SendEvent(apuToggle);
        }

        public void ApuStart()
        {
            //sch.SimConnect?.SendEvent(apuStart);
        }
    }

}
