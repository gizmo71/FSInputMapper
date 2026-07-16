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
    public struct BeaconData
    {
        [SimVar("L:S_OH_EXT_LT_BEACON", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenix;
        [SimVar("L:INI_BEACON_LIGHT_SWITCH", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int ini;
        [SimVar("L:MSATR_ELTS_BEACON", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int atr;
        [SimVar("LIGHT BEACON", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int standard;
    };

    [Component]
    public class BeaconLightSetEvent : IEvent { public string SimEvent() => "BEACON_LIGHTS_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class BeaconLight : DataListener<BeaconData>, IRequestDataOnOpen, ISettable<bool?>
    {
        private readonly BeaconLightSetEvent setEvent;
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "beaconLight";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, BeaconData data)
        {
            bool value;
            if (sc.IsFenix)
                value = data.fenix == 1;
            else if (sc.IsIniBuilds)
                value = data.ini == 1;
            else if (sc.IsAtr)
                value = data.atr == 1;
            else
                value = data.standard == 1;
            hub.Clients.All.SetFromSim(GetId(), value);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? value) {
            var code = value == true ? 1u : 0u;
            if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, $"{code} (>L:INI_BEACON_LIGHT_SWITCH)");
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, $"{code} (>L:MSATR_ELTS_BEACON)");
            else
                simConnect.SendEvent(setEvent, code);
        }
    }
}
