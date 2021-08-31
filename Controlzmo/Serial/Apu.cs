using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

// Bleed button states: `L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
// Toggle bleed with 0/`1 (>L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`) Bool; also `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
namespace Controlzmo.Serial
{
    [Component]
    public class ApuFault : LVar, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuFault(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
        }

        protected override string LVarName() => "A32NX_OVHD_APU_MASTER_SW_PB_HAS_FAULT";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set { base.Value = value; send(); } }

        private void send() => serial.SendLine("ApuFault=" + (Value == 1 ? "true" : "false"));
    }

    [Component]
    public class ApuMasterOn : LVar, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuMasterOn(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
        }

        protected override string LVarName() => "A32NX_OVHD_APU_MASTER_SW_PB_IS_ON";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set { base.Value = value; send(); } }

        private void send() => serial.SendLine("ApuMasterOn=" + (Value == 1 ? "true" : "false"));
    }

    [Component]
    public class ApuMasterButton : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public ApuMasterButton(IServiceProvider sp)
        {
            sender = sp.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "apuMasterPressed";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            sender.Execute(simConnect, $"1 (L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON, Bool) - (>L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON)");
        }
    }

    [Component]
    public class ApuAvail : LVar, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuAvail(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
        }

        protected override string LVarName() => "A32NX_OVHD_APU_START_PB_IS_AVAILABLE";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set { base.Value = value; send(); } }

        private void send() => serial.SendLine("ApuAvail=" + (Value == 1 ? "true" : "false"));
    }

    [Component]
    public class ApuStartOn : LVar, IOnSimStarted
    {
        private readonly SerialPico serial;

        public ApuStartOn(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
        }

        protected override string LVarName() => "A32NX_OVHD_APU_START_PB_IS_ON";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set { base.Value = value; send(); } }

        private void send() => serial.SendLine("ApuStartOn=" + (Value == 1 ? "true" : "false"));
    }

    [Component]
    public class ApuStartButton : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public ApuStartButton(IServiceProvider sp)
        {
            sender = sp.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "apuStartPressed";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            sender.Execute(simConnect, "(L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE, Bool) if { 1 (>L:A32NX_OVHD_APU_START_PB_IS_ON, Bool) }");
        }
    }
}
