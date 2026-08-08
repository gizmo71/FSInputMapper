using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct YellowElectricPumpData
    {
        [SimVar("L:INI_ENG2_YELLOW_ELEC_PUMP_PB", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isOnIni;
        [SimVar("L:I_OH_HYD_YELLOW_ELEC_PUMP_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isFaultFenix;
        [SimVar("L:I_OH_HYD_YELLOW_ELEC_PUMP_L", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isOnFenix;
    };

    [Component, RequiredArgsConstructor]
    public partial class YellowElectricPumpButton : DataListener<YellowElectricPumpData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "yellowElectricPump";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, YellowElectricPumpData data) {
            var isFault = false;
            var isOn = false;
            if (simConnect.IsIniBuilds) isOn = data.isOnIni == 1;
            else if (simConnect.IsFenix) { isOn = data.isOnFenix != 0; isFault = data.isFaultFenix != 0; }
            string colour = "";
            if (isFault) colour = "red";
            else if (isOn) colour = "white";
            hub.Clients.All.SetColour(GetId(), colour);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool isPressed) {
            if (simConnect.IsIniBuilds && isPressed)
                sender.Execute(simConnect, "(L:INI_ENG2_YELLOW_ELEC_PUMP_PB) ! (>L:INI_ENG2_YELLOW_ELEC_PUMP_PB)");
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_OH_HYD_YELLOW_ELEC_PUMP) ++ (>L:S_OH_HYD_YELLOW_ELEC_PUMP)");
        }
    }
}
