using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Controlzmo.Systems.Atc
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AtcFlightNumberData
    {
        // "ATC ID" is the aircraft's registration, e.g. "G-IZMO", as a fall back if flight number is blank.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        [SimVar("ATC FLIGHT NUMBER", null, SIMCONNECT_DATATYPE.STRING8, 0.0f)]
        public string flight; // SDK says up to 6 characters, but 7 works too.
    };

    // The A32NX gets this wrong when importing from SimBrief, so we fix it when we spot it.
    [Component]
    public class AtcFlightNumberListener : DataListener<AtcFlightNumberData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly Regex badPattern;

        public AtcFlightNumberListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
            badPattern = new Regex(@"^[A-Z]{3}\d+[A-Z]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override async void Process(ExtendedSimConnect simConnect, AtcFlightNumberData data)
        {
            if (badPattern.IsMatch(data.flight))
            {
//var was = data.flight;
                data.flight = "4DG";
//hub.Clients.All.Speak($"{was} becoming {data.flight}");
                simConnect.SendDataOnSimObject(data);
            }
//else hub.Clients.All.Speak($"just {data.flight}");
        }
    }
}
