using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;
using Lombok.NET;
using Microsoft.Extensions.Logging;

namespace Controlzmo.Views
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CameraStateData
    {
        [SimVar("CAMERA STATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 cameraState;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class CameraState : DataListener<CameraStateData>, IOnSimStarted
    {
        public static readonly int COCKPIT = 2;
        public static readonly int CHASE = 3;
        public static readonly int FIXED = 5;
        public static readonly int SHOWCASE = 9;

        private readonly ILogger<CameraState> log;
        public CameraStateData Current = new CameraStateData();

        public override void Process(ExtendedSimConnect simConnect, CameraStateData data)
        {
            Current = data;
            log.LogCritical($"camera state {Current.cameraState}");
        }

        public void OnStarted(ExtendedSimConnect simConnect)
        {
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.VISUAL_FRAME);
        }
    }

#if false
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CameraViewData
    {
        [SimVar("CAMERA VIEW TYPE AND INDEX:0", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 viewType;
        [SimVar("CAMERA VIEW TYPE AND INDEX:1", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 viewIndex;
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class CameraView : DataListener<CameraViewData>, IAxisCallback<T16000mHotas>
    {
        private readonly ILogger<CameraView> log;

        public override void Process(ExtendedSimConnect simConnect, CameraViewData data)
        {
            log.LogCritical($"camera view {data.viewType}/{data.viewIndex}");
        }

        public int GetAxis() => T16000mHotas.AXIS_WHEEL;

        public void OnChange(ExtendedSimConnect simConnect, double old, double @new)
        {
            simConnect.RequestDataOnSimObject(this, @new > 0 ? SIMCONNECT_CLIENT_DATA_PERIOD.VISUAL_FRAME : SIMCONNECT_CLIENT_DATA_PERIOD.NEVER);
        }
    }
#endif
}
