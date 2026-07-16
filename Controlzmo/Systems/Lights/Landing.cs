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
/*FBW is a hideous mess...
        // 2 retracted, 1 off, 0 on
        [SimVar("L:LIGHTING_LANDING_2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingSwitchLeft;
        [SimVar("L:LIGHTING_LANDING_3", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int landingSwitchRight;
        [SimVar("L:LANDING_2_RETRACTED", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int leftRetracted;
        [SimVar("L:LANDING_3_RETRACTED", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int rightRetracted;
        // Technically the circuits shouldn't come on until the lights are fully extended.
        // Can't quite make this work like the in-game panel switches.
        [SimVar("CIRCUIT SWITCH ON:18", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int leftCircuit;
        [SimVar("CIRCUIT SWITCH ON:19", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int rightCircuit;*/
        [SimVar("L:S_OH_EXT_LT_LANDING_L", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenixLeft;
        [SimVar("L:S_OH_EXT_LT_LANDING_R", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenixRight;
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
            bool value;
            if (sc.IsFenix)
                value = data.fenixLeft == 2 && data.fenixRight == 2;
            else
//TODO: all the others, and make reusable maps
                value = false;
            hub.Clients.All.SetFromSim(GetId(), value);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            if (simConnect.IsFBW) {
                /*int code = value ? 0 : 2;
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
