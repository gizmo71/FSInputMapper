using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RunwayTurnoffLightData
    {
        [SimVar("LIGHT TAXI:2", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int leftSwitch;
        [SimVar("LIGHT TAXI:3", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int rightSwitch;
        [SimVar("LIGHT TAXI ON:2", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int leftState;
        [SimVar("LIGHT TAXI ON:3", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int rightState;
    };

    [Component]
    public class RunwayTurnoffLightListener : DataListener<RunwayTurnoffLightData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger _logging;

        public RunwayTurnoffLightListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<RunwayTurnoffLightListener> _logging)
        {
            this.hub = hub;
            this._logging = _logging;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, RunwayTurnoffLightData data)
        {
            _logging.LogDebug($"Runway turnoff light state L {data.leftState} R {data.rightState}, switch L {data.leftSwitch}, switch R {data.rightSwitch}");
            hub.Clients.All.SetFromSim("lightsRunwayTurnoff", data.leftSwitch == 1 || data.rightSwitch == 1);
        }
    }

    [Component]
    public class TaxiTurnoffLightsSetter : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public TaxiTurnoffLightsSetter(JetBridgeSender sender)
        {
            this.sender = sender;
        }

        public string GetId() => "lightsRunwayTurnoff";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
        {
            sender.Execute(simConnect, "2 (>K:TOGGLE_TAXI_LIGHTS) 3 (>K:TOGGLE_TAXI_LIGHTS)");
        }
    }
}
