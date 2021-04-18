using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
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
        private readonly LVarRequester lvarRequester;

        bool? wasAirspeedAlive = null;
        bool? wasAbove80 = null;
        bool? wasAbove100 = null;
        bool? wasAboveV1 = null;
        bool? wasAboveVR = null;

        public TakeOffListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            localVarsListener = serviceProvider.GetRequiredService<LocalVarsListener>();
            lvarRequester = serviceProvider.GetRequiredService<LVarRequester>();
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }
 
        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            SIMCONNECT_PERIOD period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            wasAirspeedAlive = wasAbove80 = wasAbove100 = wasAboveV1 = wasAboveVR = null;
            if (isOnGround)
            {
                //TODO: maybe only request when airspeed comes alive, and just once?
                lvarRequester.Request(simConnect, "AIRLINER_VR_SPEED", 4000, -1.0);
                lvarRequester.Request(simConnect, "AIRLINER_V1_SPEED", 1000, -1.0);
                lvarRequester.Request(simConnect, "XMLVAR_Autobrakes_Level", 167, -1.0);
                lvarRequester.Request(simConnect, "XMLVAR_A320_WeatherRadar_Sys", 4000, -1.0);
                lvarRequester.Request(simConnect, "A32NX_SWITCH_RADAR_PWS_Position",4000, -1.0);
                lvarRequester.Request(simConnect, "A32NX_SWITCH_TCAS_Position", 4000, -1.0);
                lvarRequester.Request(simConnect, "A32NX_SWITCH_TCAS_Traffic_Position", 4000, -1.0);
            }
            else
            {
                //TODO: cancel requesting
            }
        }

        public override void Process(ExtendedSimConnect simConnect, TakeOffData data)
        {
//System.Console.Error.WriteLine($"Takeoff: KIAS {data.kias}, V1/VR {localVarsListener.localVars.v1}/{localVarsListener.localVars.vr}");
            if (data.kias < 39)
                wasAirspeedAlive = wasAbove80 = wasAboveV1 = wasAboveVR = false;
            setAndCallIfRequired(40, data.kias, "airspeed alive", ref wasAirspeedAlive);
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
