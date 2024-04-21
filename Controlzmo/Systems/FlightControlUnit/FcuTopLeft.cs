using Controlzmo.Serial;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.FlightControlUnit
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuTopLeftData
    {
        [SimVar("AUTOPILOT MANAGED SPEED IN MACH", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isMach;
        [SimVar("L:B_FCU_SPEED_MACH", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isMachFenix;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuDisplayTopLeft : DataListener<FcuTopLeftData>, ITrkFpaListener
    {
        private readonly SerialPico serial;
        private readonly FcuDisplayTopRight trkFpaHolder;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuTopLeftData data)
        {
            if (simConnect.IsFenix)
                data.isMach = data.isMachFenix;

            var speedMachLabel = data.isMach == 1 ? " MACH" : "SPD  ";
            var hdgTrkLabel = trkFpaHolder.IsTrkFpa ? "  TRK" : "HDG  ";
            var line1 = $"{speedMachLabel}  {hdgTrkLabel} LAT";
            serial.SendLine($"fcuTL={line1}");
        }
    }
}
