using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
using vJoy.Wrapper;

namespace Controlzmo.Views
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ResetViewData
    {
        [SimVar("CAMERA STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraState;
        [SimVar("CAMERA SUBSTATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraSubState;
// These are no good because they do NOT correspond to the user-assignable QuickViews - nothing in that popup widget does. :-(
// The reverse is CAMERA ACTION COCKPIT VIEW SAVE:index (index is 0-9), but there doesn't seem to be a way to recover it
// https://mtasobo.cloud.answerhub.com/questions/10056/custom-cameras-saveload.html
        [SimVar("CAMERA VIEW TYPE AND INDEX:0", null, SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraViewType;
        [SimVar("CAMERA VIEW TYPE AND INDEX:1", null, SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraViewIndex;
        [SimVar("CHASE CAMERA ZOOM", "percent", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 chaseCameraZoom;
        [SimVar("COCKPIT CAMERA ZOOM", "percent", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cockpitCameraZoom;
// Some of these might be interesting: COCKPIT CAMERA HEADLOOK (1 free 2 head), COCKPIT CAMERA HEIGHT (50% is "normal"), COCKPIT CAMERA ZOOM (default 50%)
        [SimVar("CAMERA REQUEST ACTION", null, SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 resetAction;
        [SimVar("PLANE ALT ABOVE GROUND", "feet", SIMCONNECT_DATATYPE.INT32, 50f)]
        public Int32 terrainHeight;
    }

    [Component]
    public class ResetView : DataListener<ResetViewData>
    {
        private readonly ILogger _logger;
        private readonly VirtualJoystick vJoy;

        public ResetView(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<ResetView>>();
            vJoy = new VirtualJoystick(1);
            vJoy.Aquire();
        }

        public override void Process(ExtendedSimConnect simConnect, ResetViewData data)
        {
_logger.LogCritical($"reset view {data.cameraState}/{data.cameraSubState} type/index {data.cameraViewType}/{data.cameraViewIndex} zoom {data.cockpitCameraZoom}/{data.chaseCameraZoom} above ground {data.terrainHeight}");
            if (data.cameraState == 2) // Cockpit
            {
                //data.cameraSubState = 1;
                //data.cameraViewType = 1;
                //data.cameraViewIndex = 1; // Doesn't seem to take effect. :-(
                //data.cockpitCameraZoom = 50;
                vJoy.SetJoystickButton(true, 1);
                vJoy.SetJoystickButton(false, 1);
            }
            else if (data.cameraState == 3)
            {
                //data.chaseCameraZoom = 50;
                //simConnect.SendEvent(setEvent, 0);
            }
            data.resetAction = 1;
            simConnect.SendDataOnSimObject(data);
        }
    }
}
