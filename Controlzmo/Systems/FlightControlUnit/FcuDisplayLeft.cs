﻿using Controlzmo.Serial;
using Microsoft.Extensions.DependencyInjection;
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
        [SimVar("L:A32NX_TRK_FPA_MODE_ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int isTrkFpaMode;
        [SimVar("AUTOPILOT MANAGED SPEED IN MACH", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int isMach;
    };

    [Component]
    public class FcuDisplayTopLeft : DataListener<FcuTopLeftData>, IOnSimStarted
    {
        private readonly SerialPico serial;

        public FcuDisplayTopLeft(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
        }

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SIM_FRAME);

        public override void Process(ExtendedSimConnect _, FcuTopLeftData data)
        {
            var speedMachLabel = data.isMach == 1 ? " MACH" : "SPD  ";
            var hdgTrkLabel = data.isTrkFpaMode == 0 ? "HDG  " : "  TRK";
            var line1 = $"{speedMachLabel}  {hdgTrkLabel} LAT";
            serial.SendLine($"fcuTL={line1}");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuBottomLeftData
    {
        // This is the value actually shown on the FCU, even if it's a temporary selection.
        [SimVar("L:A32NX_AUTOPILOT_SPEED_SELECTED", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.005f)]
        public float selectedSpeed;
        [SimVar("L:A32NX_FCU_SPD_MANAGED_DOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedSpeed;
        [SimVar("L:A32NX_AUTOPILOT_HEADING_SELECTED", "degrees", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)] // Gets confused when flipping between TRK and HDG...
        public float selectedHeading;
        [SimVar("L:A32NX_FCU_HDG_MANAGED_DASHES", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)] // ... so we need this to definitively decide.
        public Int32 isHeadingDashes;
        [SimVar("L:A32NX_FCU_HDG_MANAGED_DOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isManagedHeading;
    };

    [Component]
    public class FcuDisplayBottomLeft : DataListener<FcuBottomLeftData>, IOnSimStarted
    {
        private readonly SerialPico serial;

        public FcuDisplayBottomLeft(IServiceProvider sp)
        {
            serial = sp.GetRequiredService<SerialPico>();
        }

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SIM_FRAME);

        public override void Process(ExtendedSimConnect _, FcuBottomLeftData data)
        {
            var speedDot = data.isManagedSpeed == 1 ? '\x1' : ' ';
            var heading = data.isHeadingDashes == 1 || data.selectedHeading == -1.0 ? "---" : $"{data.selectedHeading!:000}";
            var headingDot = data.isManagedHeading == 1 ? '\x1' : ' ';
            var line2 = $"{Speed(data)} {speedDot}  {heading}   {headingDot} ";
            serial.SendLine($"fcuBL={line2}");
        }

        private static string Speed(FcuBottomLeftData data)
        {
            if (data.selectedSpeed >= 0.10 && data.selectedSpeed < 1.0)
                return $"{data.selectedSpeed:0.00}";
            else if (data.selectedSpeed >= 100 && data.selectedSpeed < 1000) // Max 399 in the A32NX
                return $" {data.selectedSpeed:000}";
            else
                return " ---";
        }
    }
}
