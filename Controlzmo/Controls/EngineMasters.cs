using Lombok.NET;

namespace Controlzmo.Controls
{
    [Component, RequiredArgsConstructor]
    public partial class EngineMasters
    {
        /*TODO: each engine master sends all of:
           SET ENGINE n FUEL VALVE
           SET STARTER 1
           SET ENGINE MASTER 1
        Do we really need these mapped at all, or is it easier to just click in game?
        What else could we use those switches for which would be more useful in critical phases of flight?
        It seems like maybe they do that continuously, but that's probably wrong. */
    }
}
