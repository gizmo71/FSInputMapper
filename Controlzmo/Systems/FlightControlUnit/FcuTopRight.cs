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
    public struct FcuTopRightData
    {
        [SimVar("L:A32NX_TRK_FPA_MODE_ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isTrkFpaMode;
    };

    [Component]
    public class FcuDisplayTopRight : DataListener<FcuTopRightData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;

        public FcuDisplayTopRight(IServiceProvider sp) =>serial = sp.GetRequiredService<SerialPico>();

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect _, FcuTopRightData data)
        {
            var line1 = "ALT \x4LVL/CH\x5 " + (data.isTrkFpaMode == 0 ? "V/S" : "FPA");
            serial.SendLine($"fcuTR={line1}");
        }
    }
}
