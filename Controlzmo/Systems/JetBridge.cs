using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct JetBridgeData
    {
        [MarshalAs(UnmanagedType.I4)]
        [ClientVar(0.5f)]
        public Int32 id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [ClientVar(0f)]
        public string data;
    };

    [Component]
    public class JetBridgeListener : DataListener<JetBridgeData>, IClientData, IRequestDataOnOpen
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private const string UplinkClientDataName = "theomessin.jetbridge.uplink";

        public string GetClientDataName() => UplinkClientDataName;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => (SIMCONNECT_PERIOD)SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET;

        public override void Process(ExtendedSimConnect simConnect, JetBridgeData data)
        {
System.Console.Error.WriteLine($"JetBridge reply ID {data.id} = '{data.data}'");
        }
    }
}
