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
    public struct NoseLightData
    {
        [SimVar("LIGHT TAXI:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseTaxiSwitch;
        [SimVar("LIGHT TAXI ON:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseTaxiState;
        [SimVar("LIGHT LANDING:1", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseTakeOffSwitch;
        [SimVar("LIGHT LANDING ON:1", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int noseTakeOffState;
    };

    [Component]
    public class NoseLightSystem : DataListener<NoseLightData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger _logging;
        private readonly JetBridgeSender sender;

        public NoseLightSystem(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<NoseLightSystem> _logging, JetBridgeSender sender)
        {
            this.hub = hub;
            this._logging = _logging;
            this.sender = sender;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, NoseLightData data)
        {
            _logging.LogDebug($"Nose light state taxi {data.noseTaxiState} TO {data.noseTakeOffState}, switch taxi {data.noseTaxiSwitch} TO {data.noseTakeOffSwitch}");
            oldPosition = (data.noseTaxiSwitch == 1 ? 1 : 0) + (data.noseTakeOffSwitch == 1 ? 2 : 0);
            hub.Clients.All.SetFromSim(GetId(), oldPosition);
        }

        private int oldPosition;

        public string GetId() => "lightsNose";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            int newPosition = int.Parse(value!);
            if (((newPosition ^ oldPosition) & 1) != 0)
                sender.Execute(simConnect, "1 (>K:TOGGLE_TAXI_LIGHTS)");
            if (((newPosition ^ oldPosition) & 2) != 0)
                sender.Execute(simConnect, "1 (>K:LANDING_LIGHTS_TOGGLE)");
        }
    }
}
