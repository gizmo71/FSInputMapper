using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

// Bleed button states: `L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
// Toggle bleed with 0/`1 (>L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`) Bool; also `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
namespace Controlzmo.Serial
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuFaultData
    {
        [SimVar("L:A32NX_OVHD_APU_MASTER_SW_PB_HAS_FAULT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuFault;
        [SimVar("L:I_OH_ELEC_APU_MASTER_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuFaultFenix;
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
    };

    [Component]
    public class ApuMasterOn : DataListener<ApuMasterData>, IOnSimStarted
    {
        private readonly SerialPico serial;
        public ApuMasterOn(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuMasterData data) => serial.SendLine("ApuMasterOn=" + (data.isApuMasterOn == 1 || data.isApuMasterOnFenix == 1));
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
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuAvailData
    {
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvail;
        [SimVar("L:I_OH_ELEC_APU_START_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvailFenix;
    };

    [Component]
    public class ApuAvail : DataListener<ApuAvailData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;

        public ApuAvail(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;
        public override void Process(ExtendedSimConnect simConnect, ApuAvailData data) => serial.SendLine("ApuAvail=" + (data.isApuAvail == 1 || data.isApuAvailFenix == 1));
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuStartOnData
    {
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOn;
        [SimVar("L:I_OH_ELEC_APU_START_L", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOnFenix;
    };

    [Component]
    public class ApuStartOn : DataListener<ApuStartOnData>, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuStartOn(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuStartOnData data) => serial.SendLine("ApuStartOn=" + (data.isApuStartOn == 1 || data.isApuStartOnFenix == 1));
    }

    [Component]
    public class ApuStartButton : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;
        public ApuStartButton(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();
        public string GetId() => "apuStartPressed";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            sender.Execute(simConnect, "(L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE, Bool) if { 1 (>L:A32NX_OVHD_APU_START_PB_IS_ON, Bool) }");
        }
    }
}
