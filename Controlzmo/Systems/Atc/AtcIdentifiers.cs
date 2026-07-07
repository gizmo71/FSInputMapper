using Controlzmo.Hubs;
using Lombok.NET;
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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        [SimVar("ATC FLIGHT NUMBER", null, SIMCONNECT_DATATYPE.STRING8, 0.0f)]
        public string flightNo; // SDK says up to 6 characters, but 7 works too.
    };

    internal class FullCallsign
    {
        private static readonly Regex badPattern = new Regex(@"^[A-Z]{3}(\d+[A-Z]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal static string? getFlightNumberPartIfFull(string flightNumber)
        {
            var match = badPattern.Match(flightNumber);
            return match.Success ? match.Groups[1].Captures[0].Value : null;
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class AtcFlightNumber : DataListener<AtcFlightNumberData>, IRequestDataOnOpen, ISettable<string>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, AtcFlightNumberData data)
        {
            // The A32NX gets this wrong when importing from SimBrief, so we fix it when we spot it.
            var partialFromFull = FullCallsign.getFlightNumberPartIfFull(data.flightNo);
            if (partialFromFull != null)
                SetInSim(simConnect, partialFromFull);
            else
                hub.Clients.All.SetFromSim(GetId(), data.flightNo);
        }

        public string GetId() => "flightNo";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            var data = new AtcFlightNumberData
            {
                flightNo = value?.ToUpper() ?? "4DG"
            };
            simConnect.SendDataOnSimObject(data);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AtcTailNumberData
    {
        // You *can* set this using one of these objects, but it won't affect the skin except at load time (possibly by forcing a "restart").
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        [SimVar("ATC ID", null, SIMCONNECT_DATATYPE.STRING32, 0.1f)]
        public string tailNumber; // May not be a default for some liveries; for some it's junk.
    };

    [Component, RequiredArgsConstructor]
    public partial class AtcTailNumber : DataListener<AtcTailNumberData>, IRequestDataOnOpen, ISettable<string>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, AtcTailNumberData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.tailNumber);
        }

        public string GetId() => "tailNumber";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            value = value?.Trim();
            if (value != null)
                simConnect.SendDataOnSimObject(new AtcTailNumberData { tailNumber = value.ToUpper() });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AtcCallsignData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [SimVar("ATC AIRLINE", null, SIMCONNECT_DATATYPE.STRING64, 0.1f)]
        public string airlineName;
    };

    [Component, RequiredArgsConstructor]
    public partial class AtcCallsign : DataListener<AtcCallsignData>, IRequestDataOnOpen, ISettable<string>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly AtcAirlineListener sops;

        private string _current = "<loading>";
        internal string Current {  get => _current; }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, AtcCallsignData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.airlineName.Trim());
            sops.Callsign = data.airlineName.Trim().ToLower();
            sops.OnAircraftLoaded(simConnect);
        }

        public string GetId() => "airlineName";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            value = value?.Trim();
            if (value != null)
                simConnect.SendDataOnSimObject(new AtcCallsignData { airlineName = value });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AtcHeavyData
    {
        [SimVar("ATC HEAVY", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isHeavy;
    };

    [Component, RequiredArgsConstructor]
    public partial class AtcHeavy : DataListener<AtcHeavyData>, IRequestDataOnOpen, ISettable<bool?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, AtcHeavyData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.isHeavy != 0);
        }

        public string GetId() => "isheavy";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            if (value != null)
                simConnect.SendDataOnSimObject(new AtcHeavyData { isHeavy = value == true ? 1 :0 });
        }
    }
}
