using System.Timers;
using Controlzmo;

namespace SimConnectzmo
{
    [Component]
    public class EnsureConnectionTimer : System.Timers.Timer
    {
        public EnsureConnectionTimer(Adapter adapter) : base(5000)
        {
            this.Elapsed += (object sender, ElapsedEventArgs args) => adapter.EnsureConnectionIfPossible();
        }
    }
}
