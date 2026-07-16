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
    public struct RunwayTurnoffData
    {
        [SimVar("L:S_OH_EXT_LT_RWY_TURNOFF", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenix;
        [SimVar("L:INI_TURNOFF_LIGHT_SWITCH", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int ini;
        [SimVar("LIGHT TAXI:2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fbwLeft;
        [SimVar("LIGHT TAXI:3", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fbwRight;
    }

    [Component]
    public class TaxiLightSetEvent : IEvent { public string SimEvent() => "TAXI_LIGHTS_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class RunwayTurnoffLightSystem : DataListener<RunwayTurnoffData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly TaxiLightSetEvent rtEvent;
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "lightsRunwayTurnoff";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, RunwayTurnoffData data)
        {
            bool value;
            if (sc.IsFenix)
                value = data.fenix == 1;
            else if (sc.IsIniBuilds)
                value = data.ini == 1;
            else if (sc.IsFBW)
                value = data.fbwLeft == 1 && data.fbwRight == 1;
            else
                value = false;
            hub.Clients.All.SetFromSim(GetId(), value);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            uint code = value ? 1u : 0u;
            if (simConnect.IsFBW) {
                simConnect.SendEventEx1(rtEvent, code, 2);
                simConnect.SendEventEx1(rtEvent, code, 3);
            }
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_RWY_TURNOFF)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, $"{code} (>L:INI_TURNOFF_LIGHT_SWITCH)");
//TODO: other aircraft don't really have them - combine into the taxi light system?
        }
    }
}
