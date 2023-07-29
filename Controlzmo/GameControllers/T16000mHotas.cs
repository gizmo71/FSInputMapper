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

        private static readonly int AXIS_MINISTICK_HORIZONTAL = 0;
        private static readonly int AXIS_MINISTICK_VERTICAL = 1;
        private static readonly int AXIS_THROTTLE = 2;
        private static readonly int AXIS_RUDDER_PADDLES = 5;
        private static readonly int AXIS_WHEEL = 7; // ThrustMaster call it "Antenna".

        private static readonly int BUTTON_BOTTOM_HAT_FORE = 11;
        private static readonly int BUTTON_BOTTOM_HAT_AFT = 13;
        protected override void OnUpdate(ExtendedSimConnect simConnect)
        {
            /*for (int i = 0; i < axesOld.Length; ++i)
                if (axesOld[i] != axesNew[i])
                    _log.LogDebug($"HOTAS: axes[{i}] {axesOld[i]} -> {axesNew[i]}");*/
            if (!buttonsOld[BUTTON_BOTTOM_HAT_FORE] && buttonsNew[BUTTON_BOTTOM_HAT_FORE]) {
                _log.LogDebug("User has asked for less speedbrake");
                simConnect.RequestDataOnSimObject(lessListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
            }
            if (!buttonsOld[BUTTON_BOTTOM_HAT_AFT] && buttonsNew[BUTTON_BOTTOM_HAT_AFT])
            {
                _log.LogDebug("User has asked for more speedbrake");
                simConnect.RequestDataOnSimObject(moreListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
            }
        }

    }
}
