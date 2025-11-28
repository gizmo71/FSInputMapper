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
        [SimVar("CAMERA SUBSTATE", "enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 subState;
    }

    [Component, RequiredArgsConstructor]
    public partial class CameraState : DataListener<CameraStateData>, IOnSimStarted
    {
        public static readonly int COCKPIT = 2;
        public static readonly int CHASE = 3;
        public static readonly int DRONE = 4;
        public static readonly int FIXED = 5;
        public static readonly int SHOWCASE = 9;
        public static readonly int WORLD_MAP = 12; // Yep, sometimes it gets stuck on this!

        private readonly ILogger<CameraState> log;
        private readonly SimConnectHolder holder;

        private CameraStateData _current = new CameraStateData();

        public int Current
        {
            get => _current.cameraState;
            set => holder.SimConnect!.SendDataOnSimObject(new CameraStateData { cameraState = value });
        }

        public override void Process(ExtendedSimConnect simConnect, CameraStateData data)
        {
            _current = data;
            log.LogError($"camera state {_current.cameraState} substate {_current.subState}");
        }

        public void OnStarted(ExtendedSimConnect simConnect)
        {
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.VISUAL_FRAME);
        }
    }

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
    public partial class CameraView : DataListener<CameraViewData>, IOnSimStarted
    {
        private readonly ILogger<CameraView> log;

        [Property]
        private CameraViewData _current;

        public override void Process(ExtendedSimConnect simConnect, CameraViewData data)
        {
            log.LogCritical($"camera view {data.viewType}/{data.viewIndex}");
            _current = data;
        }

        public void OnStarted(ExtendedSimConnect simConnect)
        {
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.VISUAL_FRAME);
        }
    }
}
