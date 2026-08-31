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
        // Boeings do then invididually
        [SimVar("L:XMLVAR_TRK_MODE_ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isTrkB787;
        [SimVar("L:XMLVAR_FPA_MODE_ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isFpaB787;
    };

    interface ITrkFpaListener : IRequestDataOnOpen { }

    [Component, RequiredArgsConstructor]
    public partial class FcuDisplayTopRight : DataListener<FcuTopRightData>, IRequestDataOnOpen
    {
        private readonly IServiceProvider serviceProvider;

        [Property]
        private bool _isTrk = false;
        [Property]
        private bool _isFpa = false;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuTopRightData data)
        {
            // Normalise to 787
            if (simConnect.IsFenix)
                data.isTrkB787 = data.isFpaB787 = data.isTrkFpaModeFenix;
            else if (simConnect.IsIniBuilds)
                data.isTrkB787 = data.isFpaB787 = data.isTrkFpaModeIni;
            _isTrk = data.isTrkB787 == 1;
            _isFpa = data.isFpaB787 == 1;

            var line1 = "ALT \x4LVL/CH\x5 " + (_isFpa ? "FPA" : "V/S");
            //TODO: show somewhere?

            foreach (var listener in serviceProvider.GetServices<ITrkFpaListener>()) {
                simConnect.RequestDataOnSimObject(listener, SIMCONNECT_CLIENT_DATA_PERIOD.NEVER);
                simConnect.RequestDataOnSimObject(listener, listener.GetInitialRequestPeriod());
            }
        }
    }
}
