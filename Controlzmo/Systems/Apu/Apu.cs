using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Apu
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuMasterData
    {
        [SimVar("L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOn;
        [SimVar("L:I_OH_ELEC_APU_MASTER_L", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOnFenix;
        [SimVar("L:INI_APU_MASTER_SWITCH", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOnIni;
        [SimVar("L:A32NX_OVHD_APU_MASTER_SW_PB_HAS_FAULT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuFault;
        [SimVar("L:I_OH_ELEC_APU_MASTER_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuFaultFenix;
        [SimVar("L:INI_APU_MASTER_FAULT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuFaultIni; // Not sure this can ever be set!
    };

    [Component, RequiredArgsConstructor]
    public partial class ApuMasterButton : DataListener<ApuMasterData>, IRequestDataOnOpen, ISettable<object?>
    {
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        [Property]
        private Boolean _isOn;

        public string GetId() => "apuMasterPress";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, ApuMasterData data) {
            if (simConnect.IsFenix)
            {
                data.isApuFault = data.isApuFaultFenix;
                data.isApuMasterOn = data.isApuMasterOnFenix;
            }
            else if (simConnect.IsIniBuilds)
            {
                data.isApuFault = data.isApuFaultIni;
                data.isApuMasterOn = data.isApuMasterOnIni;
            }
            _isOn = data.isApuMasterOn == 1 && data.isApuFault == 0;
            string colour = "black";
            if (data.isApuFault != 0) colour = "red";
            else if (_isOn) colour = "blue";
            hub.Clients.All.SetColour(GetId(), colour);
        }

        public void SetInSim(ExtendedSimConnect simConnect, object? value) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_OH_ELEC_APU_MASTER) ! (>L:S_OH_ELEC_APU_MASTER)");
            else if (simConnect.IsFBW)
                sender.Execute(simConnect, "1 (L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON, Bool) - (>L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "(L:INI_APU_MASTER_SWITCH_CMD) ! (>L:INI_APU_MASTER_SWITCH_CMD)");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuStartData
    {
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOn;
        [SimVar("L:I_OH_ELEC_APU_START_L", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOnFenix;
        [SimVar("L:INI_APU_START_BUTTON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOnIni;
        [SimVar("L:INI_APU_N1", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apuN1;
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvail;
        [SimVar("L:I_OH_ELEC_APU_START_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvailFenix;
        [SimVar("L:INI_APU_AVAILABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvailIni;
    }

    [Component, RequiredArgsConstructor]
    public partial class ApuStartButton : DataListener<ApuStartData>, IRequestDataOnOpen, ISettable<object?>
    {
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        [Property]
        private Boolean _isAvail;

        public string GetId() => "apuStartPress";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, ApuStartData data) {
            if (simConnect.IsFenix)
            {
                data.isApuAvail = data.isApuAvailFenix;
                data.isApuStartOn = data.isApuStartOnFenix;
            }
            else if (simConnect.IsIniBuilds)
            {
                data.isApuAvail = data.isApuAvailIni;
                data.isApuStartOn = data.isApuStartOnIni * (data.apuN1 < 95 ? 1 : 0);
            }
            _isAvail = data.isApuAvail == 1;
            string colour = "black";
            if (_isAvail) colour = "green";
            else if (data.isApuStartOn != 0) colour = "blue";
            hub.Clients.All.SetColour(GetId(), colour);
        }

        public void SetInSim(ExtendedSimConnect simConnect, object? value) {
            if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_OH_ELEC_APU_START) 2 + (>L:S_OH_ELEC_APU_START)");
            else if (simConnect.IsFBW)
                sender.Execute(simConnect, "(L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE, Bool) if { 1 (>L:A32NX_OVHD_APU_START_PB_IS_ON, Bool) }");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_APU_START_BUTTON_CMD)");
        }
    }
}
