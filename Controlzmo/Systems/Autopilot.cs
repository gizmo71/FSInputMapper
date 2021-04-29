using System;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Autopilot
{
    [Component]
    public class Autopilot1Active : LVar, IOnSimConnection
    {
        public Autopilot1Active(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOPILOT_1_ACTIVE";
        protected override int Milliseconds() => 167;
        protected override double Default() => -1.0;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);
    }

    [Component]
    public class Autopilot2Active : LVar, IOnSimConnection
    {
        public Autopilot2Active(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOPILOT_2_ACTIVE";
        protected override int Milliseconds() => 167;
        protected override double Default() => -1.0;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);
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
        private readonly JetBridgeSender jetbridge;

        public AutopilotOnEvent(IServiceProvider sp)
        {
            ap1Active = sp.GetRequiredService<Autopilot1Active>();
            ap2Active = sp.GetRequiredService<Autopilot2Active>();
            ap1Push = sp.GetRequiredService<Autopilot1OnPush>();
            ap2Push = sp.GetRequiredService<Autopilot2OnPush>();
            jetbridge = sp.GetRequiredService<JetBridgeSender>();
        }

        public string SimEvent() => "AUTOPILOT_ON";
        public IEvent GetEvent() => this;

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            double? ap1Value = ap1Active;
System.Console.Error.WriteLine($"AP On; AP1 {ap1Value} want? {ap1Active == 0.0}, AP2 {(double?)ap2Active} want? {ap2Active == 0.0}");
            if (ap1Value == 0.0)
                simConnect.SendEvent(ap1Push);
            else if (ap2Active == 0.0)
                simConnect.SendEvent(ap2Push);
            // For non-A32NX, resend normal event.
            simConnect.SendEvent(this);
        }
    }
}
