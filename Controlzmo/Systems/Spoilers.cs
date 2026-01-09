using Controlzmo.GameControllers;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using SimConnectzmo;
using System;
using System.Threading;

namespace Controlzmo.Systems.Spoilers
{
/* Asobo B38M UI code (event HANDLING_SPEED_BRAKES) is:
   p0 100 * 0 max 100 min 0 100 (F:Clamp)
   (>O:XMLVAR_Speedbrakes_Position, percent) (O:XMLVAR_Speedbrakes_Position, percent) sp0
   l0 15 > (>K:SPOILERS_ARM_SET)
   l0 30 80 0 1 (F:MapRange) 16384 * (>K:SPOILERS_SET)
   l0 30 > if{ (E:SIMULATION TIME, second) (>L:1:XMLVAR_Speedbrakes_Activation_Time) } */

    [Component] public class SpoilerArmOnEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_ON"; }
    [Component] public class SpoilerArmOffEvent : IEvent { public string SimEvent() => "SPOILERS_ARM_OFF"; }
    [Component] public class SetSpoilerHandleEvent : IEvent { public string SimEvent() => "SPOILERS_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class GroundSpoilerArming : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly SpoilerArmOffEvent armOffEvent;
        private readonly SpoilerArmOnEvent armOnEvent;
        public int GetButton() => UrsaMinorThrottle.BUTTON_GROUND_SPOILERS_ARM;
        public void OnPress(ExtendedSimConnect simConnect) => simConnect.SendEvent(armOnEvent);
        public void OnRelease(ExtendedSimConnect simConnect) => simConnect.SendEvent(armOffEvent);
    }

    [Component, RequiredArgsConstructor]
    public partial class Speedbrakes : IAxisCallback<UrsaMinorThrottle>
    {
        private readonly SetSpoilerHandleEvent _event;
        private readonly FenixSpeedbrakeHandle fenix;
        public int GetAxis() => UrsaMinorThrottle.AXIS_SPEEDBRAKES;
        public void OnChange(ExtendedSimConnect simConnect, double old, double @new) {
            if (simConnect.IsFenix)
                fenix.SetSpoilerHandleEvent(simConnect, @new);
            else
                simConnect.SendEvent(_event, (uint)(@new * 16383));
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class FenixSpeedbrakeHandle
    {
        private readonly JetBridgeSender sender;
        private const double NO_NEW_VALUE = -1.0;
        private double position = NO_NEW_VALUE;

        internal void SetSpoilerHandleEvent(ExtendedSimConnect simConnect, double position)
        {
            this.position = 1 + 2 * position;
            sender.Execute(simConnect, Execute);
        }

        private String? Execute(ExtendedSimConnect simConnect) {
            var position = Interlocked.Exchange(ref this.position, NO_NEW_VALUE);
            return position != NO_NEW_VALUE ? $"{position} (>L:A_FC_SPEEDBRAKE)" : null;
        }
    }
}
