using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    internal enum CLIENT_ENUM { PLACEHOLDER = 123 }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LocalVarsData
    {
        public Int16 v1;
        public Int16 vr;
        public Byte autobrake;
        public Byte radar;
        [MarshalAs(UnmanagedType.I1)]
        public bool pws;
        public Byte tcas;
    };

    [Component]
    public class LocalVarsListener
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] //TODO: is this needed?
        private const string VSpeedsClientDataName = "Controlzmo.VSpeeds";

        internal LocalVarsData localVars;

        internal void Wurbleise(ExtendedSimConnect simConnect)
        {
            simConnect.MapClientDataNameToID(VSpeedsClientDataName, CLIENT_ENUM.PLACEHOLDER);
System.Console.Error.WriteLine($"Mapped client data name {simConnect.GetLastSentPacketID()}");
            simConnect.CreateClientData(CLIENT_ENUM.PLACEHOLDER, (uint)Marshal.SizeOf(typeof(LocalVarsData)), SIMCONNECT_CREATE_CLIENT_DATA_FLAG.DEFAULT);
System.Console.Error.WriteLine($"Created client data {simConnect.GetLastSentPacketID()}");
            simConnect.RegisterStruct<SIMCONNECT_RECV_CLIENT_DATA, LocalVarsData>(CLIENT_ENUM.PLACEHOLDER);

            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_INT16, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_INT16, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_INT8, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_INT8, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_INT8, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_INT8, 0.5f, SimConnect.SIMCONNECT_UNUSED);
System.Console.Error.WriteLine($"Added double to client data def {simConnect.GetLastSentPacketID()}");

            simConnect.OnRecvClientData += GotSome;
            simConnect.RequestClientData(CLIENT_ENUM.PLACEHOLDER, CLIENT_ENUM.PLACEHOLDER, CLIENT_ENUM.PLACEHOLDER,
                SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET, SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
        }

        private void GotSome(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
        {
System.Console.Error.WriteLine($"Got me some client data, request ID {data.dwRequestID}; define ID {data.dwDefineID}; object {data.dwObjectID}");
            switch ((CLIENT_ENUM)data.dwRequestID)
            {
                case CLIENT_ENUM.PLACEHOLDER:
                    localVars = (LocalVarsData)data.dwData[0];
System.Console.Error.WriteLine($"LVars updated to autobrake {localVars.autobrake}; radar/PWS {localVars.radar}/{localVars.pws}, TCAS {localVars.tcas}");
                    break;
            }
        }
    }
}
