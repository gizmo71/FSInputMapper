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
            else if (sc.IsFBW)
                value = data.fbwLeft == 0 && data.fbwRight == 0;
            else if (sc.IsIni321 || sc.IsIni320)
                value = data.iniLeft == 0 && data.iniRight == 0;
            else if (sc.IsIniBuilds)
                value = data.ini != 0;
            else if (sc.IsAtr)
                value = data.atrLeft == 1 && data.atrRight == 1;
            hub.Clients.All.SetFromSim(GetId(), value);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            if (simConnect.IsA380X)
                sender.Execute(simConnect, $"{(value ? 1 : 0)} (>B:LIGHTING_LANDING_2_SET)");
            else if (simConnect.IsFBW)
            {
                /*TODO: int code = value ? 0 : 2;
                int retracted = value ? 0 : 1;
                int circuit = value ? 1 : 0;
                simConnect.SendDataOnSimObject(new LandingLightData() {
                    landingSwitchLeft = code,
                    landingSwitchRight = code,
                    leftRetracted = retracted,
                    rightRetracted = retracted,
                    leftCircuit = circuit,
                    rightCircuit = circuit,
                });*/
            }
            else if (simConnect.IsFenix)
            {
                int code = value ? 2 : 0;
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_LANDING_L) {code} (>L:S_OH_EXT_LT_LANDING_R) {code} (>L:S_OH_EXT_LT_LANDING_BOTH) ");
            }
            else if (simConnect.IsIni330)
            {
                int code = value ? 1 : 0;
                sender.Execute(simConnect, $"{code} (>L:INI_LANDING_LIGHT_SWITCH)");
            }
            else if (simConnect.IsIniBuilds)
            {
                int code = value ? 0 : 2;
                sender.Execute(simConnect, $"{code} (>L:A320_LANDING_LIGHT_SWITCH_LEFT) {code} (>L:A320_LANDING_LIGHT_SWITCH_RIGHT)");
            }
            else if (simConnect.IsAtr)
            {
                int code = value ? 1 : 0;
                sender.Execute(simConnect, $"{code} (>L:MSATR_ELTS_LDG_LEFT) {code} (>L:MSATR_ELTS_LDG_RIGHT)");
            }
        }
    }
}
