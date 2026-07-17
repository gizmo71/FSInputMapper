using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct NoseLightData
    {
        [SimVar("L:S_OH_EXT_LT_NOSE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenix;
        [SimVar("L:INI_TAXI_LIGHT_SWITCH", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 ini;
        [SimVar("L:MSATR_ELTS_TAXI_TO", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 atr;
        [SimVar("L:LIGHTING_LANDING_1", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fbw;
        [SimVar("LIGHT TAXI", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standard;
    }

    [Component]
    public class LandingLightSetEvent : IEvent { public string SimEvent() => "LANDING_LIGHTS_SET"; }

    [Component]
    [RequiredArgsConstructor]
    public partial class NoseLightSystem : DataListener<NoseLightData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly LandingLightSetEvent landingLightEvent;
        private readonly TaxiLightSetEvent taxiLightEvent;
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "lightsNose";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, NoseLightData data)
        {
            string position;
            if (sc.IsFenix)
                position = data.fenix == 0 ? "off" : (data.fenix == 1 ? "taxi" : "takeoff");
            else if (sc.IsIniBuilds)
                position = data.ini switch { 0 => "takeoff", 1 => "taxi", _ => "off" };
            else if (sc.IsAtr)
                position = data.atr == 1 ? "taxi" : "off";
            else if (sc.IsFBW)
                position = data.fbw switch { 0 => "takeoff", 1 => "taxi", _ => "off" };
            else
                position = data.standard == 0 ? "off" : "taxi";
            hub.Clients.All.SetFromSim(GetId(), position);
        }

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            if (simConnect.IsA380X)
            {
                uint code = value switch { "takeoff" => 0u, "taxi" => 1u, _ => 2u };
                sender.Execute(simConnect, $"{code} (>B:LIGHTING_LANDING_1_SET)");
            }
            else if (simConnect.IsFBW)
            {
                uint taxi = 1u;
                uint landing = 0u;
                if (value == "off")
                    taxi = 0u;
                else if (value == "takeoff")
                    landing = 1u;
                else if (value != "taxi")
                    throw new ArgumentException($"Unknown nose light value '{value}'");

                simConnect.SendEventEx1(taxiLightEvent, taxi, 1);
                simConnect.SendEventEx1(landingLightEvent, landing, 1);
            }
            else if (simConnect.IsFenix)
            {
                uint code = value switch { "takeoff" => 2u, "taxi" => 1u, _ => 0u };
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_NOSE)");
            }
            else if (simConnect.IsIniBuilds)
            {
                uint code = value switch { "takeoff" => 0u, "taxi" => 1u, _ => 2u };
                sender.Execute(simConnect, $"{code} (>L:INI_TAXI_LIGHT_SWITCH)");
            }
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, $"{(value == "off" ? 0 : 1)} (>L:MSATR_ELTS_TAXI_TO)");
        }
    }
}
