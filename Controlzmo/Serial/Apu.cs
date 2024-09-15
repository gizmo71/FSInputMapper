using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Reflection;
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
        public override void Process(ExtendedSimConnect simConnect, ApuMasterData data) {
            if (simConnect.IsFenix) data.isApuMasterOn = data.isApuMasterOnFenix;
            serial.SendLine("ApuMasterOn=" + (data.isApuMasterOn == 1));
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
        public override void Process(ExtendedSimConnect simConnect, ApuAvailData data) {
            if (simConnect.IsFenix) data.isApuAvail = data.isApuAvailFenix;
            serial.SendLine("ApuAvail=" + (data.isApuAvail == 1));
        }
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
        public override void Process(ExtendedSimConnect simConnect, ApuStartOnData data) {
            if (simConnect.IsFenix) data.isApuStartOn = data.isApuStartOnFenix;
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
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ApuBleedData
    {
        [SimVar("L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOn;
        [SimVar("L:S_OH_PNEUMATIC_APU_BLEED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuBleedOnFenix;
        [SimVar("L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOn;
        [SimVar("L:I_OH_ELEC_APU_MASTER_L", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuMasterOnFenix;
        [SimVar("L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvail;
        [SimVar("L:I_OH_ELEC_APU_START_U", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isApuAvailFenix;
        [SimVar("ABSOLUTE TIME", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 3.5f)]
        public Double nowSeconds;
    };
/*TODO:
   Bleed button states: `L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
   Fenix: 'L:I_OH_PNEUMATIC_APU_BLEED_L', 'L:I_OH_PNEUMATIC_APU_BLEED_U'
   Toggle bleed with 0/`1 (>L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`) Bool; also `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
   Fenix: '(L:S_OH_PNEUMATIC_APU_BLEED) ! (>L:S_OH_PNEUMATIC_APU_BLEED)' (is this genuinely a 'latched' switch? yes!)
   Would like to detect when AVAIL comes up, bleed isn't on, and perhaps are on the ground, and start a timer.
   Then after 1-3 minutes, turn bleed on... unless AVAIL is lost, then stop timer.
   If AVAIL and bleed are both on, and master is not on, turn the bleed off immediately.
   Note that if you turn master/start on and then master off before avail, when it cycles start off you get a moment of avail being on!
*/
    [Component]
    [RequiredArgsConstructor]
    public partial class ApuBleedMonitor : DataListener<ApuBleedData>, IOnSimStarted
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly JetBridgeSender sender;

        private Double? apuBleedOnAfter = null;

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, ApuBleedData data) {
            // Normalise...
            if (simConnect.IsFenix) {
                data.isApuBleedOn = data.isApuBleedOnFenix;
                data.isApuMasterOn = data.isApuMasterOnFenix;
                data.isApuAvail = data.isApuAvailFenix;
            }

            if (data.isApuMasterOn == 1 && data.isApuAvail == 1)
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
            else if (data.isApuMasterOn == 0 && data.isApuAvail == 1 && data.isApuBleedOn == 1)
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
            if (lvar != null)
            {
                hubContext.Clients.All.Speak("A-P-U bleed coming " + (isDemanded ? "on" : "off"));
                var value = isDemanded ? 1 : 0;
                sender.Execute(simConnect, $"{value} (>L:{lvar})");
            }
        }
    }
}
