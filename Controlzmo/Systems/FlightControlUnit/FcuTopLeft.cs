using Controlzmo.Serial;
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
        public Int32 isTrkFpaMode;
        [SimVar("AUTOPILOT MANAGED SPEED IN MACH", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isMach;
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
}
