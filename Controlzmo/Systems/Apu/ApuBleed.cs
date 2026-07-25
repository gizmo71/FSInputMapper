using Controlzmo.Systems.JetBridge;
using Controlzmo.Systems.PilotMonitoring;
using Lombok.NET;
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
        [SimVar("L:INI_APU_BLEED_BUTTON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOnIni;
        [SimVar("ABSOLUTE TIME", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 3.5f)]
        public Double nowSeconds;
    };

    [Component, RequiredArgsConstructor]
    public partial class ApuBleedMonitor : DataListener<ApuBleedData>, IOnSimStarted
    {
        private readonly Speech speech;
        private readonly JetBridgeSender sender;
        private readonly ApuMasterButton masterButton;
        private readonly ApuStartButton startButton;

        private Double? apuBleedOnAfter = null;

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, ApuBleedData data) {
            // Normalise...
            if (simConnect.IsFenix) data.isApuBleedOn = data.isApuBleedOnFenix;
            if (simConnect.IsIniBuilds) data.isApuBleedOn = data.isApuBleedOnIni;

            if (masterButton.IsOn && startButton.IsAvail)
            {
                if (data.isApuBleedOn == 0)
                {
                    if (apuBleedOnAfter == null)
                        apuBleedOnAfter = data.nowSeconds + 6/*0.0*/;
                    else if (data.nowSeconds > apuBleedOnAfter)
                        setBleed(simConnect, true);
                }
                else
                    apuBleedOnAfter = null;
            }
            else if (!startButton.IsAvail && (startButton.IsAvail || simConnect.IsA380X) && data.isApuBleedOn == 1)
            {
                setBleed(simConnect, false);
            }
        }
        
        private void setBleed(ExtendedSimConnect simConnect, Boolean isDemanded)
        {
            String? lvar = null;
            if (simConnect.IsFBW) {
                lvar = "A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON";
            } else if (simConnect.IsFenix)
                lvar = "S_OH_PNEUMATIC_APU_BLEED";
            else if (simConnect.IsIniBuilds)
                lvar = "INI_APU_BLEED_BUTTON";
            if (lvar != null)
            {
                speech.Say("A-P-U bleed coming " + (isDemanded ? "on" : "off"));
                var value = isDemanded ? 1 : 0;
                sender.Execute(simConnect, $"{value} (>L:{lvar})");
            }
        }
    }
}
