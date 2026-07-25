using Lombok.NET;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public partial class LightState
    {
        [Property]
        internal bool _isLandingOn;
        [Property]
        internal bool _isTaxiOn;
    }
}
