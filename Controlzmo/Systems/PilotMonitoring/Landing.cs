using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [Component]
    public class AutobrakeSetting : LVar
    {
        public AutobrakeSetting(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "XMLVAR_Autobrakes_Level";
        protected override int Milliseconds() => 0;
        protected override double Default() => -1.0;
    }

    [Component]
    public class AutobrakeDecelLight : LVar, IOnSimConnection
    {
        public AutobrakeDecelLight(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOBRAKES_DECEL_LIGHT";
        protected override int Milliseconds() => 1000;
        protected override double Default() => -1.0;

        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingData
    {
        [SimVar("ACCELERATION BODY Z", "feet per second squared", SIMCONNECT_DATATYPE.FLOAT64, 0.25f)]
        public double accelerationZ;
        [SimVar("SPOILERS LEFT POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT32, 0.05f)]
        public float spoilersLeft;
        [SimVar("SPOILERS RIGHT POSITION", "Percent Over 100", SIMCONNECT_DATATYPE.FLOAT32, 0.05f)]
        public float spoilersRight;
        [SimVar("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 kias;
        [SimVar("TURB ENG REVERSE NOZZLE PERCENT:1", "percent", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 rev1;
        [SimVar("TURB ENG REVERSE NOZZLE PERCENT:2", "percent", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 rev2;
    };

    [Component]
    public class LandingListener : DataListener<LandingData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly AutobrakeSetting autobrakeSetting;
        private readonly AutobrakeDecelLight autobrakeDecelLight;

        private bool? wasDecel = null;
        private bool? wasSpoilers = null;
        private bool? wasRevGreen = null;
        bool? wasBelow70 = null;

        public LandingListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
            autobrakeSetting = serviceProvider.GetRequiredService<AutobrakeSetting>();
            autobrakeDecelLight = serviceProvider.GetRequiredService<AutobrakeDecelLight>();
        }

        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            SIMCONNECT_PERIOD period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            wasDecel = wasBelow70 = null;
            wasSpoilers = isOnGround ? false : null;
        }

        private const double MIN_SPOILER_DEPLOYMENT = .9;

        public override void Process(ExtendedSimConnect simConnect, LandingData data)
        {
            if (wasDecel == null && data.kias >= 80)
            {
                autobrakeSetting.Request(simConnect);
                wasDecel = false;
            }
            else if (wasDecel == false)
            {
                bool isDecel = (autobrakeSetting == 0 || autobrakeDecelLight != 0) && data.accelerationZ <= -1.5;
                if (isDecel)
                {
                    hubContext.Clients.All.Speak("Decell!");
                    wasDecel = true;
                }
            }

            if (wasBelow70 == null && data.kias >= 70)
            {
                wasBelow70 = false;
            }
            else if (wasBelow70 == false && data.kias < 70)
            {
                hubContext.Clients.All.Speak("seventy knots");
                wasBelow70 = true;
            }

            if (wasSpoilers == false)
            {
                bool isSpoilers = data.spoilersLeft > MIN_SPOILER_DEPLOYMENT && data.spoilersRight > MIN_SPOILER_DEPLOYMENT;
                if (isSpoilers)
                {
                    hubContext.Clients.All.Speak("Spoilers!");
                    wasSpoilers = true;
                }
            }

            if (wasRevGreen == null && data.kias >= 50) wasRevGreen = false;
            if (wasRevGreen == false && data.rev1 > 0 && data.rev2 > 0)
            {
                hubContext.Clients.All.Speak("Rev Green");
                wasRevGreen = true;
            }
        }
    }
}
