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
    public struct StrobeLightData
    {
        [SimVar("L:STROBE_0_Auto", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fbwAuto;
        [SimVar("L:LIGHTING_STROBE_0", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fbwOn;
        [SimVar("L:S_OH_EXT_LT_STROBE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenix;
        [SimVar("L:INI_STROBE_LIGHT_SWITCH", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 ini;
        [SimVar("L:MSATR_ELTS_STROBE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 atr;
        [SimVar("LIGHT STROBE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standardSwitch;
        [SimVar("LIGHT STROBE ON", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standardOrOn;
    }

    [Component, RequiredArgsConstructor]
    public partial class StrobeLightSystem : DataListener<StrobeLightData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly JetBridgeSender sender;

        public string GetId() => "lightsStrobe";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, StrobeLightData data)
        {
            string position;
            if (sc.IsFenix)
                position = data.fenix == 2 ? "on" : (data.fenix == 1 ? "auto" : "off");
            else if (sc.IsIniBuilds)
                position = data.ini switch { 0 => "on", 1 => "auto", _ => "off" };
            else if (sc.IsAtr)
                position = data.atr switch { 1 => "on", _ => "off" };
            else if (sc.IsFBW)
                position = data.fbwOn != 0 ? "on" : (data.fbwAuto != 0 ? "auto" : "off");
            else
                position = data.standardOrOn != 0 ? "on" : (data.standardSwitch != 0 ? "auto" : "off");
            hub.Clients.All.SetFromSim(GetId(), position);
        }

        public void SetInSim(ExtendedSimConnect simConnect, string? position)
        {
            if (simConnect.IsFBW) {
                var auto = position == "auto" ? 1 : 0;
                var set = position != "off" ? 1 : 0;
                var value = position switch { "on" => 0, "auto" => 1, "off" => 2, _ => throw new ArgumentException($"Unknown strobe position {position}") };
                sender.Execute(simConnect, $"{auto} (>L:STROBE_0_Auto) {set} 0 r (>K:2:STROBES_SET) {value} (>L:LIGHTING_STROBE_0)");
            }
            else if (simConnect.IsFenix)
            {
                var code = position switch { "on" => 2, "auto" => 1, _ => 0 };
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_STROBE)");
            }
            else if (simConnect.IsIniBuilds)
            {
                var code = position switch { "on" => 0, "auto" => 1, _ => 2 };
                sender.Execute(simConnect, $"{code} (>L:INI_STROBE_LIGHT_SWITCH)");
            }
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, $"{(position == "on" ? 1 : 0)} (>L:MSATR_ELTS_STROBE)");
            else
            {
                var code = position == "on" ? 1 : 0;
                sender.Execute(simConnect, $"{code} (>K:STROBES_SET)");
            }
        }
    }
}
