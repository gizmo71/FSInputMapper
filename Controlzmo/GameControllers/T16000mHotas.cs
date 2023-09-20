using Controlzmo.Systems.Spoilers;
using Controlzmo.Views;
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
        private readonly IDataListener cockpitExternalToggle;
        private readonly IDataListener resetView;

        public T16000mHotas(IServiceProvider sp) : base(sp, 14, 8, 1)
        {
            moreListener = sp.GetRequiredService<MoreSpoiler>();
            lessListener = sp.GetRequiredService<LessSpoiler>();
            cockpitExternalToggle = sp.GetRequiredService<CockpitExternalToggle>();
            resetView = sp.GetRequiredService<ResetView>();
        }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 46727;

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

        /*1 front left red
          2 front right red
          3 front rocker up
          4 front rocker down
          6 middle big "rocker" pad up
          7 middle big "rocker" pad fore
          8 middle big "rocker" pad down
          9 middle big "rocker" pad aft
          10 bottom "castle" up
          12 bottom "castle" down */
        private static readonly int BUTTON_SIDE_RED = 0;
        private static readonly int BUTTON_MINISTICK = 5;
        private static readonly int BUTTON_BOTTOM_HAT_FORE = 11; // bottom "castle" fore
        private static readonly int BUTTON_BOTTOM_HAT_AFT = 13; // bottom "castle" aft
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
#if false
            for (int i = 0; i < this.buttonsOld.Length; ++i)
                if (buttonsOld[i] != buttonsNew[i])
                    _log.LogDebug($"Button {i} now {buttonsNew[i]}");
            for (int i = 0; i < this.axesOld.Length; ++i)
                if (axesOld[i] != axesNew[i])
                    _log.LogDebug($"Axis {i} now {axesNew[i]}");
            for (int i = 0; i < this.switchesOld.Length; ++i)
                if (switchesOld[i] != switchesNew[i])
                    _log.LogDebug($"Switch {i} now {switchesNew[i]}");
#endif
            // x/y/z, p/b/h
            // x: negative left, positive right
            // y: positive above, negative below
            // z: positive is ahead, negative is behind
            // p: positive is down, negative is up
            // h: 0 is forward, -90 left, 90 right
            if (axesNew[AXIS_WHEEL] != axesOld[AXIS_WHEEL] && false)
            {
                if (axesNew[AXIS_WHEEL] < 0.3)
                    simConnect.CameraSetRelative6DOF(0f, 20f, -100f, 20f, 0f, 180f);
                else if (axesNew[AXIS_WHEEL] > 0.3)
                    simConnect.CameraSetRelative6DOF(0f, 20f, 100f, 20f, 0f, 0f);
                else
                    simConnect.CameraSetRelative6DOF(0f, 100f, 0f, 90f, 0f, 0f);
            }
            if (!buttonsOld[BUTTON_SIDE_RED] && buttonsNew[BUTTON_SIDE_RED])
                simConnect.RequestDataOnSimObject(cockpitExternalToggle, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
            if (!buttonsOld[BUTTON_MINISTICK] && buttonsNew[BUTTON_MINISTICK])
                simConnect.RequestDataOnSimObject(resetView, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
        }

    }
}
