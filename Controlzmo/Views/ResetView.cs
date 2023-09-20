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
// CHASE CAMERA ZOOM - set to 50% or send event?
// Some of these might be interesting: COCKPIT CAMERA HEADLOOK (1 free 2 head), COCKPIT CAMERA HEIGHT (50% is "normal"), COCKPIT CAMERA ZOOM (default 50%)
    }

    // As far as we can tell, there's actually NO working support for sending the keypresses for these. :-( Perhaps we can actually send the keypresses...?
    [Component]
// Relies on mapping INPUT.CAMERA_USER_LOAD_5? Legacy,also  doesn't work: VIEW_CAMERA_SELECT_5
    public class QuickView5 : IEvent { public string SimEvent() => "VK_RMENU+VK_NUMPAD0"; }
// What event is the equivalent of "INPUT.CAMERA_USER_SAVE_5"?
// What about VK_RMENU+VK_NUMPAD0 etc

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
_logger.LogCritical($"reset view {data.cameraState}/{data.cameraSubState} {data.cameraViewType}/{data.cameraViewIndex}");
            if (data.cameraState == 2) // Cockpit
            {
//TODO: can we at least reset the zoom to 0?
                simConnect.SendEvent(setEvent);
            }
            else
                return;
            //simConnect.SendDataOnSimObject(data);
        }
    }
}
