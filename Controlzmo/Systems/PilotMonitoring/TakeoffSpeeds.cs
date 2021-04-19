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
        private const string LVar_v1Speed = "AIRLINER_V1_SPEED";
        private const string LVar_vrSpeed = "AIRLINER_VR_SPEED";

        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly LVarRequester lvarRequester;

        Int16 v1 = -1;
        Int16 vr = -1;

        bool? wasAirspeedAlive = null;
        bool? wasAbove80 = null;
        bool? wasAbove100 = null;
        bool? wasAboveV1 = null;
        bool? wasAboveVR = null;

        public TakeOffListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            lvarRequester = serviceProvider.GetRequiredService<LVarRequester>();
            lvarRequester.LVarUpdated += UpdateLVar;
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }
 
        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            SIMCONNECT_PERIOD period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            wasAirspeedAlive = wasAbove80 = wasAbove100 = wasAboveV1 = wasAboveVR = null;
        }

        private void UpdateLVar(string name, double? newValue)
        {
            if (name == LVar_v1Speed)
                v1 = (Int16?)newValue ?? -1;
            else if (name == LVar_vrSpeed)
                vr = (Int16?)newValue ?? -1;
System.Console.Error.WriteLine($"Updated LVar in TakeoffSpeeds: {name} = {newValue}; v1/vr now {v1}/{vr}");
        }

        public override void Process(ExtendedSimConnect simConnect, TakeOffData data)
        {
//System.Console.Error.WriteLine($"Takeoff: KIAS {data.kias}, V1/VR {localVarsListener.localVars.v1}/{localVarsListener.localVars.vr}");
            if (data.kias < 39)
                wasAirspeedAlive = wasAbove80 = wasAboveV1 = wasAboveVR = false;
            if (setAndCallIfRequired(40, data.kias, "airspeed alive", ref wasAirspeedAlive))
            {
                lvarRequester.Request(simConnect, LVar_v1Speed, 0, -1.0);
                lvarRequester.Request(simConnect, LVar_vrSpeed, 0, -1.0);
            }
            setAndCallIfRequired(80, data.kias, "eighty knots", ref wasAbove80);
            setAndCallIfRequired(100, data.kias, "one hundred knots", ref wasAbove100);
            setAndCallIfRequired(v1, data.kias, "vee one", ref wasAboveV1);
            setAndCallIfRequired(vr, data.kias, "rotate", ref wasAboveVR);
        }

        private bool setAndCallIfRequired(Int16 calledSpeed, Int32 actualSpeed, string call, ref bool? wasAbove)
        {
            if (wasAbove == false && calledSpeed > 0 && actualSpeed >= calledSpeed)
            {
                hubContext.Clients.All.Speak(call);
                wasAbove = true;
                return true;
            }
            return false;
        }
    }
}
