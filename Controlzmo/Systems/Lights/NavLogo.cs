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
    public struct LogoNavData
    {
        [SimVar("L:S_OH_EXT_LT_NAV_LOGO", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenix;
        [SimVar("L:INI_LOGO_LIGHT_SWITCH", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int ini;
        [SimVar("L:MSATR_ELTS_LOGO", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int atrLogo;
        [SimVar("L:MSATR_ELTS_NAV", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int atrNav;
        [SimVar("LIGHT LOGO", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int logo;
        [SimVar("LIGHT NAV", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int nav;
    };

    [Component]
    public class SetNavLightsEvent : IEvent { public string SimEvent() => "NAV_LIGHTS_SET"; }

    [Component]
    public class SetLogoLightsEvent : IEvent { public string SimEvent() => "LOGO_LIGHTS_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class LogoNavLightsSetter : DataListener<LogoNavData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly SetLogoLightsEvent setLogoLightsEvent;
        private readonly SetNavLightsEvent setNavLightsEvent;
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "lightsLogoNav";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, LogoNavData data)
        {
            bool value;
            if (sc.IsFenix)
                value = data.fenix == 1;
            else if (sc.IsIniBuilds)
                value = data.ini == 1;
            else if (sc.IsAtr)
                value = data.atrLogo == 1 && data.atrNav == 1;
            else
                value = data.logo == 1 && data.nav == 1;
            hub.Clients.All.SetFromSim(GetId(), value);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            uint state = value ? 1u : 0u;
            if (simConnect.IsFenix)
                sender.Execute(simConnect, $"{state} (>L:S_OH_EXT_LT_NAV_LOGO)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, $"{2u - state} (>L:INI_LOGO_LIGHT_SWITCH)");
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, $"{state} (>L:MSATR_ELTS_NAV) {state} (>L:MSATR_ELTS_LOGO)");
            else
            {
                simConnect.SendEvent(setLogoLightsEvent, state);
                simConnect.SendEvent(setNavLightsEvent, state);
            }
        }
    }
}
