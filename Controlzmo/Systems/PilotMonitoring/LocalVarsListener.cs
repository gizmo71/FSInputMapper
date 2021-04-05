using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    internal enum TEMP_ENUM { PLACEHOLDER = 123 }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
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
    };

    [Component]
    public class LocalVarsListener
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] // Why is this needed and how is it used?
        private const string VSpeedsClientDataName = "Controlzmo.VSpeeds";

        internal LocalVarsData localVars;

        internal void Wurbleise(ExtendedSimConnect simConnect)
        {
            simConnect.RegisterClientDataStruct(VSpeedsClientDataName, typeof(LocalVarsData), TEMP_ENUM.PLACEHOLDER);

            simConnect.OnRecvClientData += GotSome;
            simConnect.RequestClientData(TEMP_ENUM.PLACEHOLDER, TEMP_ENUM.PLACEHOLDER, TEMP_ENUM.PLACEHOLDER,
                SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET, SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
System.Console.Error.WriteLine($"Requested client data for: {simConnect.GetLastSentPacketID()}");
        }

        private void GotSome(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
        {
System.Console.Error.WriteLine($"Got me some client data, request ID {data.dwRequestID}; define ID {data.dwDefineID}; object {data.dwObjectID}");
            switch ((TEMP_ENUM)data.dwRequestID)
            {
                case TEMP_ENUM.PLACEHOLDER:
                    localVars = (LocalVarsData)data.dwData[0];
System.Console.Error.WriteLine($"LVars updated to autobrake {localVars.autobrake}; radar/PWS {localVars.radar}/{localVars.pws}, TCAS {localVars.tcas}");
                    break;
            }
        }
    }
}
