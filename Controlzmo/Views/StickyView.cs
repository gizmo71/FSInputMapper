using System;

namespace Controlzmo.Views
{
    public class ViewSticker
    {
        private DateTime glanceStarted = DateTime.UtcNow;

        public void TriggerAction() => glanceStarted = DateTime.UtcNow;
        public Boolean IsStuck() => DateTime.UtcNow > glanceStarted.AddSeconds(1);
    }
}
