using Lombok.NET;
using Controlzmo.Systems.JetBridge;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.EfisControlPanel
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FenixFdLsButtonSyncData
    {
        [SimVar("L:I_FCU_EFIS1_LS", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 leftLS;
        [SimVar("L:I_FCU_EFIS2_LS", "Bool", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 rightLS;
        [SimVar("L:I_FCU_EFIS1_FD", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fd1;
        [SimVar("L:I_FCU_EFIS2_FD", "Bool", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 fd2;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class FenixButtonSync : DataListener<FenixFdLsButtonSyncData>, IRequestDataOnOpen
    {
        private readonly JetBridgeSender sender;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, FenixFdLsButtonSyncData data)
        {
            if (data.leftLS != data.rightLS)
                pushAndRelease(simConnect, "LS");
            if (data.fd1 != data.fd2)
                pushAndRelease(simConnect, "FD");
        }

        private void pushAndRelease(ExtendedSimConnect simConnect, String what) {
            sender.Execute(simConnect, $"1 (>L:S_FCU_EFIS2_{what})");
            sender.Execute(simConnect, $"0 (>L:S_FCU_EFIS2_{what})");
        }
    }
}
