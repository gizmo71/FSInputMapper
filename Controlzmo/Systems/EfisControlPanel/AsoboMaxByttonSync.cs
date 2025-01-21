using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;

namespace Controlzmo.Systems.EfisControlPanel
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AsoboMaxByttonSyncData
    {
        [SimVar("AUTOPILOT FLIGHT DIRECTOR ACTIVE:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fd1;
        [SimVar("AUTOPILOT FLIGHT DIRECTOR ACTIVE:2", "Bool", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 fd2;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class AsoboMaxByttonSync : DataListener<AsoboMaxByttonSyncData>, IRequestDataOnOpen
    {
        private readonly JetBridgeSender sender;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, AsoboMaxByttonSyncData data)
        {
            if (data.fd1 != data.fd2)
                sender.Execute(simConnect, "2 (>K:TOGGLE_FLIGHT_DIRECTOR)");
        }
    }
}
