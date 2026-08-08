using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Controlzmo.Systems.PilotMonitoring;
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
    public struct ApuBleedData
    {
        [SimVar("L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOn;
        [SimVar("L:S_OH_PNEUMATIC_APU_BLEED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOnFenix;
        [SimVar("L:I_OH_PNEUMATIC_APU_BLEED_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedFaultFenix;
        [SimVar("L:INI_APU_BLEED_BUTTON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOnIni;
        [SimVar("ABSOLUTE TIME", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 3.5f)]
        public Double nowSeconds;
    };

    [Component, RequiredArgsConstructor]
    public partial class ApuBleedButton : DataListener<ApuBleedData>, IOnSimStarted, ISettable<object?>
    {
        private readonly JetBridgeSender sender;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "apuBleed";
        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME);

        public override void Process(ExtendedSimConnect simConnect, ApuBleedData data) {
            var isFault = false;
            if (simConnect.IsFenix) { data.isApuBleedOn = data.isApuBleedOnFenix; isFault = data.isApuBleedFaultFenix != 0; }
            if (simConnect.IsIniBuilds) data.isApuBleedOn = data.isApuBleedOnIni;
            string colour = "black";
            if (isFault) colour = "red";
            else if (data.isApuBleedOn != 0) colour = "blue";
            hub.Clients.All.SetColour(GetId(), colour);
        }

        public void SetInSim(ExtendedSimConnect simConnect, object? value) {
            if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "(L:INI_APU_BLEED_BUTTON) ! (>L:INI_APU_BLEED_BUTTON)");
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, "(L:S_OH_PNEUMATIC_APU_BLEED) ! (>L:S_OH_PNEUMATIC_APU_BLEED)");
            else if (simConnect.IsFBW)
                sender.Execute(simConnect, "(L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON) ! (>L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON)");
        }
    }
}
