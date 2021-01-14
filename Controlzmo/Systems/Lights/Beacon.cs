﻿using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
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
        private readonly IHubContext<LightHub, ILightHub> hub;
        private readonly ILogger<BeaconLightListener> _logging;

        public BeaconLightListener(IHubContext<LightHub, ILightHub> hub, ILogger<BeaconLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(ExtendedSimConnect simConnect, BeaconLightData data)
        {
            _logging.LogDebug($"Beacon state {data.beaconState}, switch {data.beaconSwitch}");
            hub.Clients.All.SetFromSim("Beacon", data.beaconSwitch == 1);
        }
    }

    [Component]
    public class ToggleBeaconLightsEvent : IEvent
    {
        public string SimEvent() { return "TOGGLE_BEACON_LIGHTS"; }
    }
}