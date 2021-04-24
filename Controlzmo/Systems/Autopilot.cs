using System;
using Controlzmo.SimConnectzmo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Autopilot
{
    [Component]
    public class Autopilot1Active : LVar
    {
        public Autopilot1Active(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOPILOT_1_ACTIVE";
        protected override int Milliseconds() => 167;
        protected override double Default() => -1.0;
    }

    [Component]
    public class Autopilot2Active : LVar
    {
        public Autopilot2Active(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOPILOT_2_ACTIVE";
        protected override int Milliseconds() => 167;
        protected override double Default() => -1.0;
    }

    [Component]
    public class Autopilot1OnPush : IEvent
    {
        public string SimEvent() => "A32NX.FCU_AP_1_PUSH";
    }

    [Component]
    public class Autopilot2OnPush : IEvent
    {
        public string SimEvent() => "A32NX.FCU_AP_2_PUSH";
    }

    [Component]
    public class AutopilotOnEvent : IEvent, IEventNotification
    {
        private readonly Autopilot1Active ap1Active;
        private readonly Autopilot2Active ap2Active;
        private readonly Autopilot1OnPush ap1Push;
        private readonly Autopilot2OnPush ap2Push;

        public AutopilotOnEvent(IServiceProvider sp)
        {
            ap1Active = sp.GetRequiredService<Autopilot1Active>();
            ap2Active = sp.GetRequiredService<Autopilot2Active>();
            ap1Push = sp.GetRequiredService<Autopilot1OnPush>();
            ap2Push = sp.GetRequiredService<Autopilot2OnPush>();
        }

        public string SimEvent() => "AUTOPILOT_ON";
        public IEvent GetEvent() => this;

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            if (ap1Active == 0.0)
                simConnect.SendEvent(ap1Push);
            else if (ap2Active == 0.0)
                simConnect.SendEvent(ap2Push);
            // For non-A32NX, resend normal event.
            simConnect.SendEvent(this);
        }
    }
}
