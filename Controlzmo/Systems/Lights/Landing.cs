using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingLightData
    {
        [SimVar("LIGHT LANDING:2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int a339;
        [SimVar("L:LIGHTING_LANDING_2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fbwLeft;
        [SimVar("L:LIGHTING_LANDING_3", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fbwRight;
        [SimVar("L:A320_LANDING_LIGHT_SWITCH_LEFT", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int iniLeft;
        [SimVar("L:A320_LANDING_LIGHT_SWITCH_RIGHT", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int iniRight;
        [SimVar("L:INI_LANDING_LIGHT_SWITCH", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int ini;
        [SimVar("L:S_OH_EXT_LT_LANDING_L", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenixLeft;
        [SimVar("L:S_OH_EXT_LT_LANDING_R", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenixRight;
        [SimVar("L:MSATR_ELTS_LDG_LEFT", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int atrLeft;
        [SimVar("L:MSATR_ELTS_LDG_RIGHT", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int atrRight;
    }

    [Component, RequiredArgsConstructor]
    public partial class LandingLightSystem : DataListener<LandingLightData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly JetBridgeSender sender;
        private readonly LightState state;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "landingLight";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, LandingLightData data)
        {
            var value = false;
            if (sc.IsFenix)
                value = data.fenixLeft == 2 && data.fenixRight == 2;
            else if (sc.IsA380X)
                value = data.fbwLeft != 0;
            else if (sc.IsA339)
                value = data.a339 == 1;
            else if (sc.IsFBW)
                value = data.fbwLeft == 0 && data.fbwRight == 0;
            else if (sc.IsIni321 || sc.IsIni320)
                value = data.iniLeft == 0 && data.iniRight == 0;
            else if (sc.IsIniBuilds)
                value = data.ini != 0;
            else if (sc.IsAtr)
                value = data.atrLeft == 1 && data.atrRight == 1;
            hub.Clients.All.SetFromSim(GetId(), state.IsLandingOn = value);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool isOn)
        {
            if (simConnect.IsA380X)
            {
                var landingCode = isOn ? 1 : 0;
                var noseCode = state.IsTaxiOn ? (isOn ? 0u : 1u) : 2u;
                sender.Execute(simConnect, $"{landingCode} (>B:LIGHTING_LANDING_2_SET) {noseCode} (>B:LIGHTING_LANDING_1_SET)");
            }
            else if (simConnect.IsA339)
            {
                var noseCode = state.IsTaxiOn ? (isOn ? 0u : 1u) : 2u;
                sender.Execute(simConnect, $"{(isOn ? 1 : 0)} d 3 r (>K:2:LANDING_LIGHTS_SET) 2 r (>K:2:LANDING_LIGHTS_SET) {noseCode} (>B:LIGHTING_LANDING_1_SET)");
            }
            else if (simConnect.IsFBW)
            {
                var noseCode = state.IsTaxiOn ? (isOn ? 0u : 1u) : 2u;
                sender.Execute(simConnect, $"{(isOn ? 0 : 2)} d (>B:LIGHTING_LANDING_2_SET) (>B:LIGHTING_LANDING_3_SET) {noseCode} (>B:LIGHTING_LANDING_1_SET)");
            }
            else if (simConnect.IsFenix)
            {
                var landingCode = isOn ? 2u : 0u;
                var noseCode = state.IsTaxiOn ? (isOn ? 2u : 1u) : 0u;
                sender.Execute(simConnect, $"{landingCode} (>L:S_OH_EXT_LT_LANDING_L) {landingCode} (>L:S_OH_EXT_LT_LANDING_R) {noseCode} (>L:S_OH_EXT_LT_NOSE)");
            }
            else if (simConnect.IsIniBuilds)
            {
                var landingCode = isOn ? 0u : 2u;
                if (simConnect.IsIni330) landingCode = 1 - landingCode / 2;
                var noseCode = state.IsTaxiOn ? (isOn ? 0u : 1u) : 2u;
                var mainRpn = simConnect.IsIni330 ? "(>L:INI_LANDING_LIGHT_SWITCH)" : "d (>L:A320_LANDING_LIGHT_SWITCH_LEFT) (>L:A320_LANDING_LIGHT_SWITCH_RIGHT)";
                sender.Execute(simConnect, $"{landingCode} {mainRpn} {noseCode} (>L:INI_TAXI_LIGHT_SWITCH)");
            }
            else if (simConnect.IsAtr)
            {
                int code = isOn ? 1 : 0;
                sender.Execute(simConnect, $"{code} (>L:MSATR_ELTS_LDG_LEFT) {code} (>L:MSATR_ELTS_LDG_RIGHT)");
            }
            state.IsLandingOn = isOn;
        }
    }
}
