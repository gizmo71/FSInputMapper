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
        [SimVar("L:LIGHTING_STROBE_0", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fbw;
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
    public partial class StrobeLightSystem : DataListener<StrobeLightData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly JetBridgeSender sender;

        public string GetId() => "lightsStrobe";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, StrobeLightData data)
        {
            if (sc.IsFenix)
                data.standardSwitch = data.fenix == 2 ? 1 : 0;
            else if (sc.IsIniBuilds)
                data.standardSwitch = data.ini == 0 ? 1: 0;
            else if (sc.IsAtr)
                data.standardSwitch = data.atr == 1 ? 1 : 0;
            else if (sc.IsFBW)
                data.standardSwitch = data.fbw == 0 ? 1 : 0;
            else if (data.standardOrOn != 0)
                data.standardSwitch = 1;
            hub.Clients.All.SetFromSim(GetId(), data.standardSwitch != 0);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool isOn)
        {
            if (simConnect.IsA380X)
                sender.Execute(simConnect, $"{(isOn ? 0 : 2)} (>B:LIGHTING_STROBE_0_SET)");
            else if (simConnect.IsFBW)
            {
                var set = isOn ? 1 : 0;
                var auto = set;
                var value = isOn ? 0 : 2;
                sender.Execute(simConnect, $"{auto} (>L:STROBE_0_Auto) {set} 0 r (>K:2:STROBES_SET) {value} (>L:LIGHTING_STROBE_0)");
            }
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, $"{(isOn ? 2 : 0)} (>L:S_OH_EXT_LT_STROBE)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, $"{(isOn ? 0 : 2)} (>L:INI_STROBE_LIGHT_SWITCH)");
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, $"{(isOn ? 1 : 0)} (>L:MSATR_ELTS_STROBE)");
            else
                sender.Execute(simConnect, $"{(isOn ? 1 : 0)} (>K:STROBES_SET)");
        }
    }
}
