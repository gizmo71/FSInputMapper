using Controlzmo.Hubs;
using SimConnectzmo;

namespace Controlzmo.Systems
{
    public abstract class AbstractButton : ISettable<object>
    {
        private readonly IEvent buttonEvent;
        public AbstractButton(IEvent buttonEvent) => this.buttonEvent = buttonEvent;
        public abstract string GetId();
        public virtual void SetInSim(ExtendedSimConnect simConnect, object? _) => simConnect.SendEvent(buttonEvent);
    }
}
