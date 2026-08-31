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
        [SimVar("L:INI_Airspeed_is_mach", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isMachIni;
        [SimVar("L:XMLVAR_AirSpeedIsInMach", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isMachB78x;
    };

    [Component, RequiredArgsConstructor]
    public partial class FcuDisplayTopLeft : DataListener<FcuTopLeftData>, ITrkFpaListener
    {
        private readonly FcuDisplayTopRight trkFpaHolder;
        [Property]
        internal bool _isMach;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuTopLeftData data)
        {
            if (simConnect.IsFenix)
                data.isMach = data.isMachFenix;
            else if (simConnect.IsIniBuilds)
                data.isMach = data.isMachIni;
            else if (simConnect.IsB78x)
                data.isMach = data.isMachB78x;
            _isMach = data.isMach == 1;

            var speedMachLabel = _isMach ? " MACH" : "SPD  ";
            var hdgTrkLabel = trkFpaHolder.IsTrk ? "  TRK" : "HDG  ";
            var line1 = $"{speedMachLabel}  {hdgTrkLabel} LAT";
            //TODO: show somewhere...
        }
    }
}
