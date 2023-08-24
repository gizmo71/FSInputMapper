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

        /* Buttons
        0 side red
        1 front left red
        2 front right red
        3 front rocker up
        4 front rocker down
        5 ministick press
        6 middle big "rocker" pad up
        7 middle big "rocker" pad fore
        8 middle big "rocker" pad down
        9 middle big "rocker" pad aft
        10 bottom "castle" up
        11 bottom "castle" fore
        12 bottom "castle" down
        13 bottom "castle" aft
        /* Axes
        3 right toe brake; 0 max->1 none
        4 left toe brake; 0 max->1 none
        /* Switches
        0 top coolie hat (directions reported as if looking directly at it)
         */
        private static readonly int AXIS_MINISTICK_HORIZONTAL = 0; // 0 left->1 right
        private static readonly int AXIS_MINISTICK_VERTICAL = 1; // 0 up->1 down
        private static readonly int AXIS_THROTTLE = 2; // 1 ide->0 max
        private static readonly int AXIS_RUDDER_PADDLES = 5; // 0 left->1 right
        private static readonly int AXIS_RUDDER_PEDDLES = 6; // 0 left->1 right
        private static readonly int AXIS_WHEEL = 7; // ThrustMaster call it "Antenna"; 0 aft->1 back.

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
            for (int i = 0; i < this.buttonsOld.Length; ++i)
                if (buttonsOld[i] != buttonsNew[i])
                    _log.LogDebug($"Button {i} now {buttonsNew[i]}");
            for (int i = 0; i < this.axesOld.Length; ++i)
                if (axesOld[i] != axesNew[i])
                    _log.LogDebug($"Axis {i} now {axesNew[i]}");
            for (int i = 0; i < this.switchesOld.Length; ++i)
                if (switchesOld[i] != switchesNew[i])
                    _log.LogDebug($"Switch {i} now {switchesNew[i]}");
        }

    }
}
