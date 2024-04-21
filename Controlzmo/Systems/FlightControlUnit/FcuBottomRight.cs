using Controlzmo.Serial;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.FlightControlUnit
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuBottomRightData
    {
        [SimVar("L:A32NX_TRK_FPA_MODE_ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isTrkFpaMode;
        [SimVar("AUTOPILOT ALTITUDE LOCK VAR:3", "feet", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fcuAlt;
        /* Seems to be (based on https://github.com/flybywiresim/a32nx/blob/42e4134f9235ff0a1842edc10aad7c56a52b7989/flybywire-aircraft-a320-neo/html_ui/Pages/VCockpit/Instruments/Airliners/FlyByWire_A320_Neo/FCU/A320_Neo_FCU.js):
         * 0 'idle' when in ALT (-----), which also triggers "managed" to be set; then for VS:
         * 1 when pushed to level ( 00oo);
         * 2 for selecting V/S and 3 after pulling (both +/-00oo).
         * In FPA, the number is always shown except for condition 0. */
        [SimVar("L:A320_NE0_FCU_STATE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vsState;
        [SimVar("L:B_FCU_VERTICALSPEED_DASHED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenixState;
        [SimVar("L:A32NX_AUTOPILOT_VS_SELECTED", "feet per minute", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 vsSelected;
        [SimVar("L:A32NX_AUTOPILOT_FPA_SELECTED", "Degrees", SIMCONNECT_DATATYPE.FLOAT32, 0.05f)]
        public float fpaSelected;
        [SimVar("L:N_FCU_VS", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenixSelected; // In FPA, this is angle in degrees multiplied by 1000
        [SimVar("L:A32NX_FCU_ALT_MANAGED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManaged;
        [SimVar("L:N_FCU_ALTITUDE", "feet", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fcuAltFenix;
        [SimVar("L:I_FCU_ALTITUDE_MANAGED", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedFenix;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuDisplayBottomRight : DataListener<FcuBottomRightData>, ITrkFpaListener
    {
        private readonly SerialPico serial;
        private readonly FcuDisplayTopRight trkFpaHolder;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuBottomRightData data)
        {
            // Normalise Fenix to FBW...
            if (simConnect.IsFenix)
            {
                data.isManaged = data.isManagedFenix;
                data.fcuAlt = data.fcuAltFenix;
                data.fpaSelected = data.fenixSelected / 1000.0f;
                data.vsSelected = data.fenixSelected;
                data.vsState = 1 - data.fenixState;
            }

            var managed = data.isManaged == 1 ? '\x1' : ' ';
            var line2 = $"{data.fcuAlt:00000}   {managed}  {VS(data)}";
            serial.SendLine($"fcuBR={line2}");
        }

        private string VS(FcuBottomRightData data)
        {
            string vs;
            if (data.vsState == 0)
                vs = "-----";
            else if (trkFpaHolder.IsTrkFpa)
                vs = $"{data.fpaSelected!:+#0.0;-#0.0} ";
            else
                vs = $"{data.vsSelected / 100:+00;-00}oo";
            return vs;
        }
    }
}
