using SimConnectzmo;

namespace Controlzmo.Hubs
{
    public interface ISettable
    {
        string GetId();
        void SetInSim(ExtendedSimConnect simConnect, bool value);
    }
}
