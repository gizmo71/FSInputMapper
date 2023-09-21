using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
//using System.Windows.Forms; https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?view=windowsdesktop-7.0 no right alt?

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
    }

    // As far as we can tell, there's actually NO working support for sending the keypresses for these. :-( Perhaps we can actually send the keypresses...?
    [Component]
    // Relies on mapping INPUT.CAMERA_USER_LOAD_5? Legacy,also  doesn't work: VIEW_CAMERA_SELECT_5
    // Note that this won't current work as we're using SimConnect_MapClientEventToSimEvent but we actually need a different sort, SimConnect_MapInputEventToClientEvent
    // But that's probably not right anyway as I think you can only ask to have sim events sent for specific input events, not inject them...
    public class QuickView5 : IEvent { public string SimEvent() => "PAN_RESET"; }
// What event is the equivalent of "INPUT.CAMERA_USER_SAVE_5"?
// What about VK_RMENU+VK_NUMPAD0 etc
// We'll probably have to use vJoy and feed it instructions, mapping it's virtual buttons to the 10 quickviews.
// Latest: https://github.com/njz3/vJoy; UDP feeder https://github.com/klach/vjoy-udp-feeder

    [Component]
    public class ResetView : DataListener<ResetViewData>
    {
        private readonly ILogger _logger;
        private readonly QuickView5 setEvent;

        public ResetView(IServiceProvider sp)
        {
            _logger = sp.GetRequiredService<ILogger<ResetView>>();
            setEvent = sp.GetRequiredService<QuickView5>();
        }

        public override void Process(ExtendedSimConnect simConnect, ResetViewData data)
        {
_logger.LogCritical($"reset view {data.cameraState}/{data.cameraSubState} type/index {data.cameraViewType}/{data.cameraViewIndex} zoom {data.cockpitCameraZoom}/{data.chaseCameraZoom}");
            if (data.cameraState == 2) // Cockpit
            {
                data.cameraSubState = 1;
                data.cameraViewType = 1;
                data.cameraViewIndex = 1; // Doesn't seem to take effect. :-(
                //data.cockpitCameraZoom = 50;
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
