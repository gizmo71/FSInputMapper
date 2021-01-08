using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.ThrustLevers
{
    [Component]
    public class AutothrustArmToggleEvent : IEvent { public string SimEvent() { return "AUTO_THROTTLE_ARM"; } }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AutothrustState
    {
        [SimVar("AUTOPILOT THROTTLE ARM", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 autothrustArmed;
    }

    [Component]
    public class ArmAutothrustListener : DataListener<AutothrustState>
    {
        private readonly AutothrustArmToggleEvent armEvent;

        public ArmAutothrustListener(AutothrustArmToggleEvent armEvent)
        {
            this.armEvent = armEvent;
        }

        public override void Process(ExtendedSimConnect simConnect, AutothrustState armingState)
        {
            // The default behaviour to toggle, not to arm, so ignore it unless not already armed.
            if (armingState.autothrustArmed == 0)
                simConnect.SendEvent(armEvent);
        }
    }

    [Component]
    public class AutothrustArmEventNotification : IEventNotification
    {
        private readonly AutothrustArmToggleEvent trigger;
        private readonly ArmAutothrustListener listener;

        public AutothrustArmEventNotification(ArmAutothrustListener listener, AutothrustArmToggleEvent trigger)
        {
            this.listener = listener;
            this.trigger = trigger;
        }

        public IEvent GetEvent() { return trigger; }

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            simConnect.RequestDataOnSimObject(listener, SIMCONNECT_PERIOD.ONCE);
        }
    }
}
