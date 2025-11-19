using Controlzmo.Hubs;
using Controlzmo.Systems.Atc;
using Controlzmo.Systems.EfisControlPanel;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
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
        [SimVar("ON ANY RUNWAY", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onAnyRunway;
        [SimVar("L:A32NX_REVERSER_1_DEPLOYED", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 rev1deployed;
        [SimVar("L:A32NX_FADEC_POWERED_ENG1", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 fadec1power;
        [SimVar("L:A32NX_REVERSER_2_DEPLOYED", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 rev2deployed;
        [SimVar("L:A320_Engine_Reverser_Left", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.4f)]
        public float rev1fenix;
        [SimVar("L:A320_Engine_Reverser_Right", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.4f)]
        public float rev2fenix;
        [SimVar("L:A32NX_FADEC_POWERED_ENG1", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 fadec2power;
        [SimVar("L:XMLVAR_Autobrakes_Level", "number", SIMCONNECT_DATATYPE.INT32, 1f)]
        public Int32 autobrakesLevel;
        [SimVar("L:A32NX_AUTOBRAKES_DECEL_LIGHT", "number", SIMCONNECT_DATATYPE.INT32, 1f)]
        public Int32 decelLight;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingListener : DataListener<LandingData>, IOnGroundHandler
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly EngineCooldownListener cooldown;

        private bool? wasDecel = null;
        private bool? wasSpoilers = null;
        private bool? wasRevGreen = null;
        bool? wasBelow70 = null;
        bool? wasBelowTaxi = null;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
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
            else if (wasBelowTaxi == false && data.onAnyRunway == 1 && data.groundSpeed < 30)
            {
                cooldown.StartTimer(simConnect);
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

            if (simConnect.IsFenix)
            {
                data.rev1deployed = data.rev1fenix == 100.0 ? 1 : 0;
                data.rev2deployed = data.rev2fenix == 100.0 ? 1 : 0;
                data.fadec1power = data.fadec2power = 1;
            }

            if (wasRevGreen == null && data.kias >= 50) wasRevGreen = false;
            if (wasRevGreen == false && data.rev1deployed == 1 && data.fadec1power == 1 && data.rev2deployed == 1 && data.fadec2power == 1)
            {
                hubContext.Clients.All.Speak("Rev Green");
                wasRevGreen = true;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineCooldownData
    {
        [SimVar("ABSOLUTE TIME", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 2.5f)]
        public Double now; // See notes in warmup version.
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class EngineCooldownListener : DataListener<EngineCooldownData>
    {
        private readonly ChronoButton chronoButton;
        private readonly AtcAirlineListener atcAirline;

        private Double? coolAt;

        internal void StartTimer(ExtendedSimConnect simConnect)
        {
            coolAt = null;
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.SECOND);
        }

        // Until/unless we discover a better to detect/predict Pegasus v2's behaviour
        private const int SLACK = 30;

        public override void Process(ExtendedSimConnect simConnect, EngineCooldownData data)
        {
            if (coolAt == null)
                coolAt = data.now + atcAirline.CooldownMinutes * 60.0 + SLACK;
            else if (data.now >= coolAt)
            {
                chronoButton.SetInSim(simConnect, null);
                coolAt = null;
                simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.NEVER);
//TODO: is there anything we could trigger after three minutes ABSOLUTE TIME to signal the end of this? APU Bleed?
            }
            else
                return;
            chronoButton.SetInSim(simConnect, null);
        }
    }
}
