using Controlzmo.Hubs;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;
using Microsoft.AspNetCore.SignalR;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AtcAirlineData
    {
        [SimVar("ATC AIRLINE", "String", SIMCONNECT_DATATYPE.STRINGV, 1.0f)]
        public string name;
    };

    [Component]
    public class AtcAirlineListener : DataListener<AtcAirlineData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public AtcAirlineListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, AtcAirlineData data)
        {
            hub.Clients.All.SetFromSim(AtcAirline.id, data.name);
        }
    }

    [Component]
    public class AtcAirline : ISettable<string>
    {
        internal const string id = "atcAirline";

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            //TODO: could we want to set this? Would it work?
        }
    }
}
