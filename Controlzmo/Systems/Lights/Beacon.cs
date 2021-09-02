using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Lights
{
    [Component]
    public class BeaconLight : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public BeaconLight(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "beaconLight";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            sender.Execute(simConnect, $"(A:LIGHT BEACON, Bool) {desiredValue} != if{{ 0 (>K:TOGGLE_BEACON_LIGHTS) }}");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BeaconLightData
    {
        [SimVar("LIGHT BEACON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int beaconSwitch;
        [SimVar("LIGHT BEACON ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int beaconState;
    };

    [Component]
    public class BeaconLightListener : DataListener<BeaconLightData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger<BeaconLightListener> _logging;

        public BeaconLightListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<BeaconLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, BeaconLightData data)
        {
            _logging.LogDebug($"Beacon state {data.beaconState}, switch {data.beaconSwitch}");
            hub.Clients.All.SetFromSim("lightsBeacon", data.beaconSwitch == 1);
        }
    }

    [Component]
    public class ToggleBeaconLightsEvent : IEvent
    {
        public string SimEvent() => "TOGGLE_BEACON_LIGHTS";
    }

    [Component]
    public class BeaconLightsSetter : ISettable<bool>
    {
        private readonly ToggleBeaconLightsEvent toggleBeaconLightsEvent;

        public BeaconLightsSetter(ToggleBeaconLightsEvent toggleBeaconLightsEvent)
        {
            this.toggleBeaconLightsEvent = toggleBeaconLightsEvent;
        }

        public string GetId() => "lightsBeacon";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            simConnect.SendEvent(toggleBeaconLightsEvent);
        }
    }
}
