using Controlzmo.Systems.Spoilers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class T16000mHotas : GameController
    {
        private readonly MoreSpoiler moreListener;
        private readonly LessSpoiler lessListener;

        public T16000mHotas(IServiceProvider sp) : base(sp, 14, 8, 1)
        {
            moreListener = sp.GetRequiredService<MoreSpoiler>();
            lessListener = sp.GetRequiredService<LessSpoiler>();
        }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 46727;

        public override void OnButtonChange(ExtendedSimConnect simConnect, int index, bool isPressed)
        {
            _log.LogDebug($"HOTAS button {index}: {isPressed}");
            if (isPressed)
            {
                switch (index)
                {
                    case 11: // Forward
                        _log.LogDebug("User has asked for less speedbrake");
                        simConnect.RequestDataOnSimObject(lessListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
                        break;
                    case 13: // Backward
                        _log.LogDebug("User has asked for more speedbrake");
                        simConnect.RequestDataOnSimObject(moreListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
                        break;
                }
            }
        }

    }
}
