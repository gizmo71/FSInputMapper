using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SeatBeltSignData
    {
        [SimVar("L:S_OH_SIGNS", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenix;
        [SimVar("L:INI_SEATBELTS_SWITCH", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int ini; // Note that 330 and others don't align!
        [SimVar("L:MSATR_CABS_SEAT_BELTS", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int atr;
        [SimVar("CABIN SEATBELTS ALERT SWITCH", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int standard;
    }

    [Component, RequiredArgsConstructor]
    public partial class SeatBeltSign : DataListener<SeatBeltSignData>, IRequestDataOnOpen, ISettable<bool?>
    {
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "seatBeltSign";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, SeatBeltSignData data)
        {
            bool _current;
            if (sc.IsFenix)
                _current = data.fenix == 1;
            else if (sc.IsA330)
                _current = data.ini == 1;
            else if (sc.IsIniBuilds)
                _current = data.ini != 2;
            else if (sc.IsAtr)
                _current = data.atr == 1;
            else
                _current = data.standard == 1;
            hub.Clients.All.SetFromSim(GetId(), _current);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            string? action = null;
            if (simConnect.IsFBW)
            {
                action = "(A:CABIN SEATBELTS ALERT SWITCH,Bool) " + desiredValue + " != if{ (>K:CABIN_SEATBELTS_ALERT_SWITCH_TOGGLE) }";
                if (simConnect.IsA380X)
                    sender.Execute(simConnect, $"{2 - desiredValue * 2} (>L:XMLVAR_SWITCH_OVHD_INTLT_SEATBELT_Position)");
            }
            else if (simConnect.IsFenix)
                action = $"{desiredValue} (>L:S_OH_SIGNS)";
            else if (simConnect.IsAtr)
                action = $"{desiredValue} (>L:MSATR_CABS_SEAT_BELTS)";
            else if (simConnect.IsIniBuilds)
            {
                desiredValue = value == true ? 0 : 2;
                if (simConnect.IsIni330) desiredValue = 1 - (desiredValue / 2);
                action = $"{desiredValue} (>L:INI_SEATBELTS_SWITCH)";
            }

            if (action != null)
                sender.Execute(simConnect, action);
        }
    }
}
