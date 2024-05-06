using Controlzmo.Serial;
using Lombok.NET;
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
        [SimVar("L:I_FCU_TRACK_FPA_MODE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isTrkFpaModeFenix;
        [SimVar("L:INI_TRACK_FPA_STATE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isTrkFpaModeIni;
    };

    interface ITrkFpaListener : IRequestDataOnOpen { }

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuDisplayTopRight : DataListener<FcuTopRightData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;
        private readonly IServiceProvider serviceProvider;

        private bool isTrkFpa = false;
        public bool IsTrkFpa { get => isTrkFpa; }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuTopRightData data)
        {
            // Normalise to FBW.
            if (simConnect.IsFenix)
                data.isTrkFpaMode = data.isTrkFpaModeFenix;
            else if (simConnect.IsIni320)
                data.isTrkFpaMode = data.isTrkFpaModeIni;
            isTrkFpa = data.isTrkFpaMode == 1;

            var line1 = "ALT \x4LVL/CH\x5 " + (isTrkFpa ? "FPA" : "V/S");
            serial.SendLine($"fcuTR={line1}");

            foreach (var listener in serviceProvider.GetServices<ITrkFpaListener>()) {
                simConnect.RequestDataOnSimObject(listener, SIMCONNECT_CLIENT_DATA_PERIOD.NEVER);
                simConnect.RequestDataOnSimObject(listener, listener.GetInitialRequestPeriod());
            }
        }
    }
}
