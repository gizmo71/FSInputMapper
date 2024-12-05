using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;

namespace Controlzmo.Systems.EfisControlPanel
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IniFdLsButtonSyncData
    {
        [SimVar("L:INI_LS_CAPTAIN", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 leftLS;
        [SimVar("L:INI_LS_FO", "Bool", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 rightLS;
        [SimVar("L:INI_FD1_ON", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fd1;
        [SimVar("L:INI_FD2_ON", "Bool", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 fd2;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class IniButtonSync : DataListener<IniFdLsButtonSyncData>, IRequestDataOnOpen
    {
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, IniFdLsButtonSyncData data)
        {
            if (data.leftLS != data.rightLS || data.fd1 != data.fd2)
            {
                data.rightLS = data.leftLS;
                data.fd2 = data.fd1;
                simConnect.SendDataOnSimObject(data);
            }
        }
    }
}
