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
namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TakeOffData
    {
        [SimVar("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 kias;
    };

    [Component]
    public class V1Speed : LVar
    {
        public V1Speed(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "AIRLINER_V1_SPEED";
        protected override int Milliseconds() => 0;
        protected override double Default() => -1.0;
    }

    [Component]
    public class VrSpeed : LVar
    {
        public VrSpeed(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "AIRLINER_VR_SPEED";
        protected override int Milliseconds() => 0;
        protected override double Default() => -1.0;
    }

    [Component]
    public class TakeOffListener : DataListener<TakeOffData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly V1Speed v1Speed;
        private readonly VrSpeed vrSpeed;

        bool? wasAirspeedAlive = null;
        bool? wasAbove80 = null;
        bool? wasAbove100 = null;
        bool? wasAboveV1 = null;
        bool? wasAboveVR = null;

        public TakeOffListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            v1Speed = serviceProvider.GetRequiredService<V1Speed>();
            vrSpeed = serviceProvider.GetRequiredService<VrSpeed>();
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
            if (data.kias < 39)
                wasAirspeedAlive = wasAbove80 = wasAboveV1 = wasAboveVR = false;
            if (setAndCallIfRequired(40, data.kias, "airspeed alive", ref wasAirspeedAlive))
            {
                v1Speed.Request(simConnect);
                vrSpeed.Request(simConnect);
            }
            _ = setAndCallIfRequired(80, data.kias, "eighty knots", ref wasAbove80);
            _ = setAndCallIfRequired(100, data.kias, "one hundred knots", ref wasAbove100);
            _ = setAndCallIfRequired((Int16?)v1Speed ?? 0, data.kias, "vee one", ref wasAboveV1);
            _ = setAndCallIfRequired((Int16?)vrSpeed ?? 0, data.kias, "rotate", ref wasAboveVR);
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
