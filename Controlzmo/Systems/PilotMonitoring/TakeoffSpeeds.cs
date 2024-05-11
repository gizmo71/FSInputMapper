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
        [SimVar("L:AIRLINER_V1_SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 v1;
        [SimVar("L:AIRLINER_VR_SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 vr;
        [SimVar("L:AIRLINER_V2_SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 v2;
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

        private SIMCONNECT_PERIOD period = SIMCONNECT_PERIOD.NEVER;

        bool? wasAbove80 = null;
        bool? wasAboveV1 = null;
        bool? wasAboveVR = null;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            period = isOnGround ? SIMCONNECT_PERIOD.ONCE : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            //TODO: also reset in case of RTO.
            wasAbove80 = wasAboveV1 = wasAboveVR = null;
        }

        public override void Process(ExtendedSimConnect simConnect, TakeOffData data)
        {
            if (data.kias < 49) {
                wasAbove80 = wasAboveV1 = wasAboveVR = false;
//TODO: do we need to call with NEVER first?
                if (period != SIMCONNECT_PERIOD.SECOND)
                    simConnect.RequestDataOnSimObject(this, period = SIMCONNECT_PERIOD.SECOND);
                hubContext.Clients.All.SetFromSim(v1Setter.GetId(), data.v1);
                hubContext.Clients.All.SetFromSim(vrSetter.GetId(), data.vr);
                hubContext.Clients.All.SetFromSim(v2Setter.GetId(), data.v2);
            }
            else if (period != SIMCONNECT_PERIOD.SIM_FRAME)
            {
//TODO: do we need to call with NEVER first?
                simConnect.RequestDataOnSimObject(this, period = SIMCONNECT_PERIOD.SIM_FRAME);
            }
            _ = SetAndCallIfRequired(80, data.kias, "eighty knots", ref wasAbove80, 0);
            if (data.vr < data.v1 + 3)
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
