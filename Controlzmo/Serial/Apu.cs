using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Serial
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

    [Component]
    public class ApuMasterOn : DataListener<ApuMasterData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;

        private Boolean _isOn;
        internal Boolean IsOn { get => _isOn; }
        public ApuMasterOn(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
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

    [Component]
    public class ApuAvail : DataListener<ApuAvailData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;

        private Boolean _isAvail;
        internal Boolean IsAvail { get => _isAvail; }

        public ApuAvail(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;
        public override void Process(ExtendedSimConnect simConnect, ApuAvailData data) {
            if (simConnect.IsFenix) data.isApuAvail = data.isApuAvailFenix;
            if (simConnect.IsIniBuilds) data.isApuAvail = data.isApuAvailIni;
            _isAvail = data.isApuAvail == 1;
            serial.SendLine($"ApuAvail={_isAvail}");
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

    [Component]
    public class ApuStartOn : DataListener<ApuStartOnData>, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuStartOn(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuStartOnData data) {
            if (simConnect.IsFenix) data.isApuStartOn = data.isApuStartOnFenix;
            if (simConnect.IsIniBuilds) data.isApuStartOn = data.isApuStartOnIni * (data.apuN1 < 95 ? 1 : 0);
            serial.SendLine("ApuStartOn=" + (data.isApuStartOn == 1));
        }
    }

    [Component]
    public class ApuStartButton : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;
        public ApuStartButton(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();
        public string GetId() => "apuStartPressed";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            if (simConnect.IsFBW)
                sender.Execute(simConnect, "(L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE, Bool) if { 1 (>L:A32NX_OVHD_APU_START_PB_IS_ON, Bool) }");
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_OH_ELEC_APU_START) 2 + (>L:S_OH_ELEC_APU_START)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "1 (>L:INI_APU_START_BUTTON_CMD)");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuBleedData
    {
        [SimVar("L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOn;
        [SimVar("L:S_OH_PNEUMATIC_APU_BLEED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOnFenix;
        [SimVar("L:INI_APU_BLEED_BUTTON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOnIni;
        [SimVar("ABSOLUTE TIME", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 3.5f)]
        public Double nowSeconds;
    };

    [Component, RequiredArgsConstructor]
    public partial class ApuBleedMonitor : DataListener<ApuBleedData>, IOnSimStarted
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly JetBridgeSender sender;
        private readonly ApuMasterOn master;
        private readonly ApuAvail avail;

        private Double? apuBleedOnAfter = null;

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, ApuBleedData data) {
            // Normalise...
            if (simConnect.IsFenix) data.isApuBleedOn = data.isApuBleedOnFenix;
            if (simConnect.IsIniBuilds) data.isApuBleedOn = data.isApuBleedOnIni;

            if (master.IsOn && avail.IsAvail)
            {
                if (data.isApuBleedOn == 0)
                {
                    if (apuBleedOnAfter == null)
                        apuBleedOnAfter = data.nowSeconds + 60.0;
                    else if (data.nowSeconds > apuBleedOnAfter)
                        setBleed(simConnect, true);
                }
                else
                    apuBleedOnAfter = null;
            }
            else if (!master.IsOn && (avail.IsAvail || simConnect.IsA380X) && data.isApuBleedOn == 1)
            {
                setBleed(simConnect, false);
            }
//else hubContext.Clients.All.Speak($"A-P-U bleed {data.isApuBleedOn} master {data.isApuMasterOn} a veil {data.isApuAvail} on after {apuBleedOnAfter != null}");
        }

        private void setBleed(ExtendedSimConnect simConnect, Boolean isDemanded)
        {
            String? lvar = null;
            if (simConnect.IsFBW)
                lvar = "A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON";
            else if (simConnect.IsFenix)
                lvar = "S_OH_PNEUMATIC_APU_BLEED";
            else if (simConnect.IsIniBuilds)
                lvar = "INI_APU_BLEED_BUTTON";
            if (lvar != null)
            {
                hubContext.Clients.All.Speak("A-P-U bleed coming " + (isDemanded ? "on" : "off"));
                var value = isDemanded ? 1 : 0;
                sender.Execute(simConnect, $"{value} (>L:{lvar})");
            }
        }
    }
}
