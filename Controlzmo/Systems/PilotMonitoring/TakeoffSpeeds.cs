using System;
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
        [SimVar("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)]
        public float kias;
    };

    [Component]
    public class TakeOffListener : DataListener<TakeOffData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly LocalVarsListener localVarsListener;

        bool? wasAirspeedAlive = null;
        bool? wasAbove80 = null;
        bool? wasAboveV1 = null;
        bool? wasAboveVR = null;

        public TakeOffListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            localVarsListener = serviceProvider.GetRequiredService<LocalVarsListener>();
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }
 
        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            simConnect.RequestDataOnSimObject(this, isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER);
            wasAirspeedAlive = wasAbove80 = wasAboveV1 = wasAboveVR = null;
        }

        public override void Process(ExtendedSimConnect simConnect, TakeOffData data)
        {
System.Console.Error.WriteLine($"Takeoff: KIAS {data.kias}, V1/VR {localVarsListener.localVars.v1}/{localVarsListener.localVars.vr}");
            if (data.kias < 30)
                wasAirspeedAlive = wasAbove80 = wasAboveV1 = wasAboveVR = false;
            setAndCallIfRequired(31, data.kias, "airspeed alive", ref wasAirspeedAlive);
            setAndCallIfRequired(80, data.kias, "eighty knots", ref wasAbove80);
            setAndCallIfRequired(localVarsListener.localVars.v1, data.kias, "vee one", ref wasAboveV1);
            setAndCallIfRequired(localVarsListener.localVars.vr, data.kias, "rotate", ref wasAboveVR);
        }

        private void setAndCallIfRequired(short calledSpeed, float actualSpeed, string call, ref bool? wasAbove)
        {
            bool isAbove = calledSpeed > 0 && actualSpeed >= calledSpeed;
            if (isAbove && wasAbove == false)
            {
                hubContext.Clients.All.Speak(call);
                wasAbove = true;
            }
        }
    }
}
