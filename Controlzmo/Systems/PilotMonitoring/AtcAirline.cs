using Controlzmo.Hubs;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AtcAirlineData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [SimVar("ATC AIRLINE", null, SIMCONNECT_DATATYPE.STRING64, 0.0f)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        [SimVar("ATC MODEL", null, SIMCONNECT_DATATYPE.STRING32, 0.0f)]
        public string model;
    };

    [Component]
    public class AtcAirlineListener : DataListener<AtcAirlineData>, IOnSimStarted
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public AtcAirlineListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public void OnStarted(ExtendedSimConnect simConnect)
        {
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);
        }

        public override void Process(ExtendedSimConnect simConnect, AtcAirlineData data)
        {
            hub.Clients.All.SetFromSim("atcAirline", $"Callsign {data.name}, type {data.model}");
        }
    }
}
