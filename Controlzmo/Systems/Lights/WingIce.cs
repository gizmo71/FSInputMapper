using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct WingIceLightData
    {
        [SimVar("LIGHT WING", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int wingSwitch;
        [SimVar("LIGHT WING ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int wingState;
    };

    [Component]
    public class WingIceLightListener : DataListener<WingIceLightData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger<WingIceLightListener> _logging;

        public WingIceLightListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<WingIceLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, WingIceLightData data)
        {
            _logging.LogDebug($"Wing ice lights state {data.wingState}");
            hub.Clients.All.SetFromSim("lightsWingIce", data.wingSwitch == 1);
        }
    }

    [Component]
    public class ToggleWingIceLightsEvent : IEvent
    {
        public string SimEvent() => "TOGGLE_WING_LIGHTS";
    }

    [Component]
    public class WingIceLightsSetter : ISettable
    {
        private readonly ToggleWingIceLightsEvent toggleWingIceLightsEvent;

        public WingIceLightsSetter(ToggleWingIceLightsEvent toggleWingIceLightsEvent)
        {
            this.toggleWingIceLightsEvent = toggleWingIceLightsEvent;
        }

        public string GetId() => "lightsWingIce";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            simConnect.SendEvent(toggleWingIceLightsEvent);
        }
    }
}
