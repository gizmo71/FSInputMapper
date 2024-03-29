﻿using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
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
    };

    [Component]
    public class TakeOffListener : DataListener<TakeOffData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        bool? wasAbove80 = null;
        bool? wasAboveV1 = null;
        bool? wasAboveVR = null;

        public TakeOffListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }
 
        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            SIMCONNECT_PERIOD period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            //TODO: also reset in case of RTO.
            wasAbove80 = wasAboveV1 = wasAboveVR = null;
        }

        public override void Process(ExtendedSimConnect simConnect, TakeOffData data)
        {
            if (data.kias < 49)
                wasAbove80 = wasAboveV1 = wasAboveVR = false;
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
