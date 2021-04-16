using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

//TODO: how do we get the "thrust set" call?
//TODO: "positive climb" would also be a PM call in some SOPs - consider go around too
//TODO: "ten thousand" call - in both directions
namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TakeOffData
    {
        [SimVar("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 kias;
    };

    [Component]
    public class TakeOffListener : DataListener<TakeOffData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly LocalVarsListener localVarsListener;

        bool? wasAirspeedAlive = null;
        bool? wasAbove80 = null;
        bool? wasAbove100 = null;
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
            SIMCONNECT_PERIOD period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            wasAirspeedAlive = wasAbove80 = wasAbove100 = wasAboveV1 = wasAboveVR = null;
        }

        public override void Process(ExtendedSimConnect simConnect, TakeOffData data)
        {
//System.Console.Error.WriteLine($"Takeoff: KIAS {data.kias}, V1/VR {localVarsListener.localVars.v1}/{localVarsListener.localVars.vr}");
            if (data.kias < 30)
                wasAirspeedAlive = wasAbove80 = wasAboveV1 = wasAboveVR = false;
            setAndCallIfRequired(31, data.kias, "airspeed alive", ref wasAirspeedAlive);
            setAndCallIfRequired(80, data.kias, "eighty knots", ref wasAbove80);
            setAndCallIfRequired(100, data.kias, "one hundred knots", ref wasAbove100);
            setAndCallIfRequired(localVarsListener.localVars.v1, data.kias, "vee one", ref wasAboveV1);
            setAndCallIfRequired(localVarsListener.localVars.vr, data.kias, "rotate", ref wasAboveVR);
        }

        private void setAndCallIfRequired(Int16 calledSpeed, Int32 actualSpeed, string call, ref bool? wasAbove)
        {
            if (wasAbove == false && calledSpeed > 0 && actualSpeed >= calledSpeed)
            {
                hubContext.Clients.All.Speak(call);
                wasAbove = true;
            }
        }
    }
}
