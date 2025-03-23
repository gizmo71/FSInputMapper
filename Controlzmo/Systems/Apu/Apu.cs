using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.Systems.Controls.Engine;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Apu
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuFaultData
    {
        [SimVar("L:A32NX_OVHD_APU_MASTER_SW_PB_HAS_FAULT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuFault;
        [SimVar("L:I_OH_ELEC_APU_MASTER_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuFaultFenix;
        [SimVar("L:INI_APU_MASTER_FAULT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuFaultIni; // Not sure this can ever be set!
    };

    [Component]
    public class ApuFault : DataListener<ApuFaultData>, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuFault(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuFaultData data) {
            Boolean state;
            if (simConnect.IsFBW)
                state = data.isApuFault == 1;
            else if (simConnect.IsFenix)
                state = data.isApuFaultFenix == 1;
            else if (simConnect.IsIniBuilds)
                state = data.isApuFaultIni == 1;
            else
                return;
            serial.SendLine("ApuFault=" + state);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuMasterData
    {
        [SimVar("L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOn;
        [SimVar("L:I_OH_ELEC_APU_MASTER_L", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOnFenix;
        [SimVar("L:INI_APU_MASTER_SWITCH", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOnIni;
    };

    [Component,  RequiredArgsConstructor]
    public partial class ApuMasterOn : DataListener<ApuMasterData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;
        [Property]
        private Boolean _isOn;
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;
        public override void Process(ExtendedSimConnect simConnect, ApuMasterData data) {
            if (simConnect.IsFenix) data.isApuMasterOn = data.isApuMasterOnFenix;
            if (simConnect.IsIniBuilds) data.isApuMasterOn = data.isApuMasterOnIni;
            _isOn = data.isApuMasterOn == 1;
            serial.SendLine($"ApuMasterOn={_isOn}");
        }
    }

    [Component]
    public class ApuMasterButton : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;
        public ApuMasterButton(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();
        public string GetId() => "apuMasterPressed";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            if (simConnect.IsFBW)
                sender.Execute(simConnect, "1 (L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON, Bool) - (>L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON)");
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_OH_ELEC_APU_MASTER) ! (>L:S_OH_ELEC_APU_MASTER)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "(L:INI_APU_MASTER_SWITCH_CMD) ! (>L:INI_APU_MASTER_SWITCH_CMD)");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuAvailData
    {
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvail;
        [SimVar("L:I_OH_ELEC_APU_START_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvailFenix;
        [SimVar("L:INI_APU_AVAILABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvailIni;
    };

    [Component] public class StartApuEvent : IEvent { public string SimEvent() => "APU_STARTER"; }
    [Component] public class StopApuEvent : IEvent { public string SimEvent() => "KEY_APU_OFF_SWITCH"; }
    [Component] public class FuelSystemPumpOnEvent : IEvent { public string SimEvent() => "FUELSYSTEM_PUMP_ON"; }
    [Component] public class FuelSystemPumpOffEvent : IEvent { public string SimEvent() => "FUELSYSTEM_PUMP_OFF"; }

    [Component, RequiredArgsConstructor]
    public partial class ApuAvail : DataListener<ApuAvailData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly StartApuEvent startApu;
        private readonly StopApuEvent stopApu;
        private readonly FuelSystemPumpOnEvent fuelPumpOn;
        private readonly FuelSystemPumpOffEvent fuelPumpOff;
        private readonly FuelSystemValveOpenEvent fuelValveOpen;
        private readonly FuelSystemValveCloseEvent fuelValveClose;

        [Property]
        private Boolean _isAvail;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, ApuAvailData data) {
            if (simConnect.IsFenix) data.isApuAvail = data.isApuAvailFenix;
            if (simConnect.IsIniBuilds) data.isApuAvail = data.isApuAvailIni;
            _isAvail = data.isApuAvail == 1;
            serial.SendLine($"ApuAvail={_isAvail}");

            if (simConnect.IsHorizonLvfr) {
                if (_isAvail) hubContext.Clients.All.Speak("Check engine masters");
                simConnect.SendEvent(_isAvail ? fuelValveOpen : fuelValveClose, 8);
                simConnect.SendEvent(_isAvail ? fuelPumpOn : fuelPumpOff, 7);
                simConnect.SendEvent(_isAvail ? startApu : stopApu, 1);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuStartOnData
    {
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOn;
        [SimVar("L:I_OH_ELEC_APU_START_L", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOnFenix;
        [SimVar("L:INI_APU_START_BUTTON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOnIni;
        [SimVar("L:INI_APU_N1", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 apuN1;
    };

    [Component, RequiredArgsConstructor]
    public partial class ApuStartOn : DataListener<ApuStartOnData>, IOnSimStarted
    {
        private readonly SerialPico serial;
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuStartOnData data) {
            if (simConnect.IsFenix) data.isApuStartOn = data.isApuStartOnFenix;
            if (simConnect.IsIniBuilds) data.isApuStartOn = data.isApuStartOnIni * (data.apuN1 < 95 ? 1 : 0);
            serial.SendLine("ApuStartOn=" + (data.isApuStartOn == 1));
        }
    }
    [Component] public class ApuStarterEvent : IEvent { public string SimEvent() => "APU_STARTER"; }

    [Component, RequiredArgsConstructor]
    public partial class ApuStartButton : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;
        private readonly ApuStarterEvent apuStarterEvent;

        public string GetId() => "apuStartPressed";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            if (simConnect.IsFBW) {
                sender.Execute(simConnect, "(L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE, Bool) if { 1 (>L:A32NX_OVHD_APU_START_PB_IS_ON, Bool) }");
                if (simConnect.IsHorizonLvfr)
                    simConnect.SendEvent(apuStarterEvent, 1);
            }  else if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_OH_ELEC_APU_START) 2 + (>L:S_OH_ELEC_APU_START)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_APU_START_BUTTON_CMD)");
        }
    }
}
