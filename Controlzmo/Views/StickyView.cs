using System;

namespace Controlzmo.Views
{
    [Component]
    public class ViewSticker
    {
        private DateTime glanceStarted = DateTime.UtcNow;

        public void TriggerStart() => glanceStarted = DateTime.UtcNow;
        public Boolean IsStuck(double millis) => DateTime.UtcNow > glanceStarted.AddMilliseconds(millis);
    }
}
