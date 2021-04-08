using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1/*, Size = 16*/)]
    public struct LocalVarsData
    {
        [MarshalAs(UnmanagedType.I2)]
        [ClientVar(0.5f)]
        public Int16 v1;
        [MarshalAs(UnmanagedType.I2)]
        [ClientVar(0.5f)]
        public Int16 vr;
        [MarshalAs(UnmanagedType.I1)]
        [ClientVar(0.5f)]
        public Byte autobrake;
        [MarshalAs(UnmanagedType.I1)]
        [ClientVar(0.5f)]
        public Byte radar;
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.I1)]
        public bool pws;
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.I1)]
        public Byte tcas;
        [MarshalAs(UnmanagedType.I1)]
        public Byte tcasTraffic;
    };

    [Component]
    public class LocalVarsListener : DataListener<LocalVarsData>, IClientData, IRequestDataOnOpen
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] // Why is this needed and how is it used?
        private const string VSpeedsClientDataName = "Controlzmo.VSpeeds";

        public LocalVarsData localVars;

        public string GetClientDataName() => VSpeedsClientDataName;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => (SIMCONNECT_PERIOD)SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET;

        public override void Process(ExtendedSimConnect simConnect, LocalVarsData localVars)
        {
System.Console.Error.WriteLine($"LVars updated to autobrake {localVars.autobrake}; V1/VR {localVars.v1}/{localVars.vr}");
System.Console.Error.WriteLine($"radar/PWS {localVars.radar}/{localVars.pws}, TCAS/traffic {localVars.tcas}/{localVars.tcasTraffic}");
            this.localVars = localVars;
        }
    }
}
