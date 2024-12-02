using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingLightData
    {
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
        public int rightCircuit;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class LandingLightSystem : ISettable<bool>, IData<LandingLightData>
    {
        private readonly JetBridgeSender sender;

        public string GetId() => "lightsLanding";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            if (simConnect.IsFBW) {
                int code = value ? 0 : 2;
                int retracted = value ? 0 : 1;
                int circuit = value ? 1 : 0;
                simConnect.SendDataOnSimObject(new LandingLightData() {
                    landingSwitchLeft = code,
                    landingSwitchRight = code,
                    leftRetracted = retracted,
                    rightRetracted = retracted,
                    leftCircuit = circuit,
                    rightCircuit = circuit,
                });
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
        }
    }
}
