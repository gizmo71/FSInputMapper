
using Controlzmo.Hubs;
using Controlzmo.Systems.EfisControlPanel;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.PilotMonitoring
{
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
        [SimVar("GPS GROUND SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 groundSpeed;
        [SimVar("L:A32NX_REVERSER_1_DEPLOYED", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 rev1deployed;
        [SimVar("L:A32NX_FADEC_POWERED_ENG1", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 fadec1power;
        [SimVar("L:A32NX_REVERSER_1_DEPLOYED", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 rev2deployed;
        [SimVar("L:A32NX_FADEC_POWERED_ENG1", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 fadec2power;
        [SimVar("L:XMLVAR_Autobrakes_Level", "number", SIMCONNECT_DATATYPE.INT32, 1f)]
        public Int32 autobrakesLevel;
        [SimVar("L:A32NX_AUTOBRAKES_DECEL_LIGHT", "number", SIMCONNECT_DATATYPE.INT32, 1f)]
        public Int32 decelLight;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingListener : DataListener<LandingData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly ChronoButton chronoButton;

        private bool? wasDecel = null;
        private bool? wasSpoilers = null;
        private bool? wasRevGreen = null;
        bool? wasBelow70 = null;
        bool? wasBelowTaxi = null;

        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            var period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            wasRevGreen = wasDecel = wasBelow70 = wasBelowTaxi = null;
            wasSpoilers = isOnGround ? false : null;
        }

        private const double MIN_SPOILER_DEPLOYMENT = .9;

        public override void Process(ExtendedSimConnect simConnect, LandingData data)
        {
            if (wasDecel == null && data.kias >= 80)
            {
                wasDecel = false;
            }
            else if (wasDecel == false)
            {
                bool isDecel = (data.autobrakesLevel == 0 || data.decelLight != 0) && data.accelerationZ <= -1.5;
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

            if (wasBelowTaxi == null && data.groundSpeed >= 70)
            {
                wasBelowTaxi = false;
            }
            else if (wasBelowTaxi == false && data.groundSpeed < 30)
            {
//TODO: is there anything we could trigger after three minutes ABSOLUTE TIME to signal the end of this? APU Bleed?
                chronoButton.SetInSim(simConnect, null);
                wasBelowTaxi = true;
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
            if (wasRevGreen == false && data.rev1deployed == 1 && data.fadec1power == 1 && data.rev2deployed == 1 && data.fadec2power == 1)
            {
                hubContext.Clients.All.Speak("Rev Green");
                wasRevGreen = true;
            }
        }
    }
}
