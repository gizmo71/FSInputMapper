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
    public struct FcuBottomLeftData
    {
        // This is the value actually shown on the FCU, even if it's a temporary selection.
        [SimVar("L:A32NX_AUTOPILOT_SPEED_SELECTED", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.005f)]
        public float selectedSpeed;
        [SimVar("L:N_FCU_SPEED", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.005f)]
        public float selectedSpeedFenix;
        [SimVar("L:INI_AIRSPEED_DIAL", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.005f)]
        public float selectedSpeedIni;
        [SimVar("L:A32NX_FCU_SPD_MANAGED_DOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedSpeed;
        [SimVar("L:I_FCU_SPEED_MANAGED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedSpeedFenix;
        [SimVar("L:INI_FCU_SPD_DOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedSpeedIni;
        [SimVar("L:B_FCU_SPEED_DASHED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isSpeedDashedFenix;
        [SimVar("L:INI_FCU_SPD_DASHED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isSpeedDashedIni;
        [SimVar("L:N_FCU_HEADING", "degrees", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)]
        public float selectedHeadingFenix;
        [SimVar("L:INI_TRK_DIAL", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)]
        public float selectedTrackIni;
        [SimVar("L:INI_HEADING_DIAL", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)]
        public float selectedHeadingIni;
        [SimVar("L:A32NX_FCU_HDG_MANAGED_DOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedHeading;
        [SimVar("L:A32NX_FCU_SELECTED_HEADING", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public float selectedHeading; // Gets confused when flipping between TRK and HDG...
        [SimVar("L:A32NX_FCU_HDG_MANAGED_DASHES", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isHeadingDashes; // ... so we need this to definitively decide.
        [SimVar("L:INI_FCU_HDG_DASHED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isHeadingDashesIni;
        [SimVar("L:B_FCU_HEADING_DASHED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isHeadingDashesFenix;
        [SimVar("L:I_FCU_HEADING_MANAGED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedHeadingFenix;
        [SimVar("L:INI_FCU_HDG_DOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedHeadingIni;
    };

    [Component, RequiredArgsConstructor]
    public partial class FcuDisplayBottomLeft : DataListener<FcuBottomLeftData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;
        private readonly FcuDisplayTopRight trkFpaHolder;
        private readonly FcuToast toast;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuBottomLeftData data)
        {
            if (simConnect.IsFenix)
            {
                data.isManagedSpeed = data.isManagedSpeedFenix;
                if (data.selectedSpeedFenix < 100)
                    data.selectedSpeedFenix /= 100.0f;
                data.selectedSpeed = data.isSpeedDashedFenix == 1 ? 0 : data.selectedSpeedFenix;
                data.isHeadingDashes = data.isHeadingDashesFenix;
                data.selectedHeading = data.selectedHeadingFenix;
                data.isManagedHeading = data.isManagedHeadingFenix;
            }
            else if (simConnect.IsIniBuilds)
            {
                data.isManagedSpeed = data.isManagedSpeedIni;
                data.selectedSpeed = data.isSpeedDashedIni == 1 ? 0 : data.selectedSpeedIni;
                data.isHeadingDashes = data.isHeadingDashesIni;
                data.selectedHeading = trkFpaHolder.IsTrkFpa ? data.selectedTrackIni : data.selectedHeadingIni;
                data.isManagedHeading = data.isManagedHeadingIni;
            }
            else
            {
                if (data.selectedHeading == -1.0) data.isHeadingDashes = 1;
            }

            var speedDot = data.isManagedSpeed == 1 ? '\x1' : ' ';
            var heading = data.isHeadingDashes == 1 ? "---" : $"{data.selectedHeading!:000}";
            var headingDot = data.isManagedHeading == 1 ? '\x1' : ' ';
            var line2 = $"{Speed(data)} {speedDot}  {heading}   {headingDot} ";
            serial.SendLine($"fcuBL={line2}");
            toast.Left = line2;
        }

        private static string Speed(FcuBottomLeftData data)
        {
            if (data.selectedSpeed >= 0.10 && data.selectedSpeed < 1.0)
                return $"{data.selectedSpeed:0.00}";
            else if (data.selectedSpeed >= 100 && data.selectedSpeed < 1000) // Max 399 in the A32NX
                return $" {Math.Floor(data.selectedSpeed):000}";
            else
                return " ---";
        }
    }
}
