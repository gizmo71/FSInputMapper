using System;
using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using Controlzmo.Systems.Spoilers;

namespace Controlzmo.Serial
{
    [Component]
    public class SpeedbrakeHandle : ISettable<Int16?>
    {
        private readonly IEvent armOn;
        private readonly IEvent armOff;
        private readonly IEvent set;

        public SpeedbrakeHandle(IServiceProvider sp)
        {
            armOn = sp.GetRequiredService<SpoilerArmOnEvent>();
            armOff = sp.GetRequiredService<SpoilerArmOffEvent>();
            set = sp.GetRequiredService<SetSpoilerHandleEvent>();
        }

        public string GetId() => "speedBrakeHandle";

        public void SetInSim(ExtendedSimConnect simConnect, Int16? value)
        {
            simConnect.SendEvent(value >= 0 ? armOff : armOn);
            uint eventData = (uint)Math.Max((int)value!, 0) * 164;
            eventData = Math.Min(eventData, 16384u);
            simConnect.SendEvent(set, eventData);
        }
    }
}
