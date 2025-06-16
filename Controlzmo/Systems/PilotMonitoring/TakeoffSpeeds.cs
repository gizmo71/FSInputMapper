using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TakeOffData
    {
        [SimVar("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 kias;
        [SimVar("L:AIRLINER_V1_SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 v1;
        [SimVar("L:AIRLINER_VR_SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vr;
        [SimVar("L:AIRLINER_V2_SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 v2;
        // These can't be written to.
        [SimVar("L:N_MISC_PERF_TO_V1", "Knots", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 v1Fenix;
        [SimVar("L:N_MISC_PERF_TO_VR", "Knots", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vrFenix;
        [SimVar("L:N_MISC_PERF_TO_V2", "Knots", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 v2Fenix;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class ToSpeedV1 : ISettable<string>
    {
        private readonly JetBridgeSender sender;
        public string GetId() => "toSpeedV1";
        public void SetInSim(ExtendedSimConnect simConnect, string? value) => sender.Execute(simConnect, $"{value.Parse(0)} (>L:AIRLINER_V1_SPEED)");
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class ToSpeedVr : ISettable<string>
    {
        private readonly JetBridgeSender sender;
        public string GetId() => "toSpeedVr";
        public void SetInSim(ExtendedSimConnect simConnect, string? value) => sender.Execute(simConnect, $"{value.Parse(0)} (>L:AIRLINER_VR_SPEED)");
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class ToSpeedV2 : ISettable<string>
    {
        private readonly JetBridgeSender sender;
        public string GetId() => "toSpeedV2";
        public void SetInSim(ExtendedSimConnect simConnect, string? value) => sender.Execute(simConnect, $"{value.Parse(0)} (>L:AIRLINER_V2_SPEED)");
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class TakeOffListener : DataListener<TakeOffData>, IOnGroundHandler
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly ToSpeedV1 v1Setter;
        private readonly ToSpeedVr vrSetter;
        private readonly ToSpeedV2 v2Setter;

        bool? wasAbove80 = null;
        bool? wasAboveV1 = null;
        bool? wasAboveVR = null;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            simConnect.RequestDataOnSimObject(this, isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER);
            //TODO: also reset in case of RTO.
            wasAbove80 = wasAboveV1 = wasAboveVR = null;
            // Some aircraft do this for us.
            if (simConnect.IsA380X || simConnect.IsIni330 || simConnect.IsIni337 || simConnect.IsA339)
                wasAboveV1 = true;
        }

        public override void Process(ExtendedSimConnect simConnect, TakeOffData data)
        {
            if (simConnect.IsFenix)
            {
                data.v1 = data.v1Fenix;
                data.vr = data.vrFenix;
                data.v2 = data.v2Fenix;
            }

            if (data.kias < 49) {
                wasAbove80 = wasAboveV1 = wasAboveVR = false;
                hubContext.Clients.All.SetFromSim(v1Setter.GetId(), data.v1);
                hubContext.Clients.All.SetFromSim(vrSetter.GetId(), data.vr);
                hubContext.Clients.All.SetFromSim(v2Setter.GetId(), data.v2);
            }

            _ = SetAndCallIfRequired(80, data.kias, "eighty knots", ref wasAbove80, 0);
            if (data.vr < data.v1 + 3 && wasAboveV1 == false)
            {
                _ = SetAndCallIfRequired(data.v1, data.kias, "vee one rotate", ref wasAboveV1, 3);
                wasAboveVR = wasAboveV1;
            }
            else
            {
                _ = SetAndCallIfRequired(data.v1, data.kias, "vee one", ref wasAboveV1, 3);
                _ = SetAndCallIfRequired(data.vr, data.kias, "rotate", ref wasAboveVR, 3);
            }
        }

        private bool SetAndCallIfRequired(Int32 calledSpeed, Int32 actualSpeed, string call, ref bool? wasAbove, int offset)
        {
            if (wasAbove == false && calledSpeed > 0 && actualSpeed >= (calledSpeed - offset))
            {
                hubContext.Clients.All.Speak(call);
                wasAbove = true;
                return true;
            }
            return false;
        }
    }
}
