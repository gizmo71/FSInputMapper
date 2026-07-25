using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct WingIceLightData
    {
        [SimVar("L:S_OH_EXT_LT_WING", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenix;
        [SimVar("LIGHT WING", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standard;
    };

    [Component]
    public class ToggleWingLightsEvent : IEvent { public string SimEvent() => "TOGGLE_WING_LIGHTS"; }

    [Component, RequiredArgsConstructor]
    public partial class WingIceLight : DataListener<WingIceLightData>, IRequestDataOnOpen, ISettable<bool?>
    {
        private readonly ToggleWingLightsEvent toggleEvent;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private bool? _current = null;

        public string GetId() => "wingIceLight";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, WingIceLightData data)
        {
            if (sc.IsFenix)
                data.standard = data.fenix;
            hub.Clients.All.SetFromSim(GetId(), _current = data.standard != 0);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            if (value == null || _current == value) return;
            simConnect.SendEvent(toggleEvent);
        }
    }
}
