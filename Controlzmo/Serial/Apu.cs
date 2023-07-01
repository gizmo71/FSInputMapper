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
    };

    [Component]
    public class ApuFault : DataListener<ApuFaultData>, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuFault(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuFaultData data) => serial.SendLine("ApuFault=" + (data.isApuFault == 1));
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuMasterData
    {
        [SimVar("L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOn;
    };

    [Component]
    public class ApuMasterOn : DataListener<ApuMasterData>, IOnSimStarted
    {
        private readonly SerialPico serial;
        public ApuMasterOn(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuMasterData data) => serial.SendLine("ApuMasterOn=" + (data.isApuMasterOn == 1));
    }

    [Component]
    public class ApuMasterButton : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;
        public ApuMasterButton(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();
        public string GetId() => "apuMasterPressed";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            sender.Execute(simConnect, $"1 (L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON, Bool) - (>L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON)");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuAvailData
    {
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvail;
    };

    [Component]
    public class ApuAvail : DataListener<ApuAvailData>, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuAvail(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuAvailData data) => serial.SendLine("ApuAvail=" + (data.isApuAvail == 1));
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuStartOnData
    {
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuStartOn;
    };

    [Component]
    public class ApuStartOn : DataListener<ApuStartOnData>, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuStartOn(IServiceProvider serviceProvider) => serial = serviceProvider.GetRequiredService<SerialPico>();
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);
        public override void Process(ExtendedSimConnect simConnect, ApuStartOnData data) => serial.SendLine("ApuStartOn=" + (data.isApuStartOn == 1));
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
