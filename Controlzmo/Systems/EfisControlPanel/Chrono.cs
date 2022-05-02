using SimConnectzmo;
using System.ComponentModel;

namespace Controlzmo.Systems.EfisControlPanel
{
    [Component]
    public class Chrono1Event : IEvent
    {
        public string SimEvent() => "A32NX.EFIS_L_CHRONO_PUSHED";
    }

    [Component]
    public class Com1Swap : AbstractButton
    {
        public Com1Swap(Chrono1Event chronoEvent) : base(chronoEvent) { }
        public override string GetId() => "chrono1press";
    }
}
