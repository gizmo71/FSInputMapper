using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TransponderStateData
    {
        [SimVar("TRANSPONDER STATE:1", "Enum", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 state;
    };

    [Component]
    public class TransponderStateListener : DataListener<TransponderStateData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TransponderStateListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, TransponderStateData data)
        {
            hub.Clients.All.SetFromSim("xpndr", data.state);
        }
    }

    [Component]
    public class TransponderState : ISettable<string?>
    {
        public string GetId() => "xpndr";

        public void SetInSim(ExtendedSimConnect simConnect, string? newModeString)
        {
            var newState = new TransponderStateData { state = Int32.Parse(newModeString!) };
            simConnect.SendDataOnSimObject(newState);
        }
    }
}
