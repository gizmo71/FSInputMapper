using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    internal enum CLIENT_ENUM { PLACEHOLDER = 123 }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct VSpeedsCallData
    {
        public double airspeed;
        public double v1;
        public double vr;
        public Int32 phase;
    };

    [Component]
    public class TakeOffSpeedsListener
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] //TODO: is this needed?
        private const string VSpeedsClientDataName = "Controlzmo.VSpeeds";

        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public TakeOffSpeedsListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        internal void Wurbleise(ExtendedSimConnect simConnect)
        {
            simConnect.MapClientDataNameToID(VSpeedsClientDataName, CLIENT_ENUM.PLACEHOLDER);
System.Console.Error.WriteLine($"Mapped client data name {simConnect.GetLastSentPacketID()}");
            simConnect.CreateClientData(CLIENT_ENUM.PLACEHOLDER, (uint)Marshal.SizeOf(typeof(VSpeedsCallData)), SIMCONNECT_CREATE_CLIENT_DATA_FLAG.DEFAULT);
System.Console.Error.WriteLine($"Created client data {simConnect.GetLastSentPacketID()}");
            simConnect.RegisterStruct<SIMCONNECT_RECV_CLIENT_DATA, VSpeedsCallData>(CLIENT_ENUM.PLACEHOLDER);

            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_FLOAT64, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_FLOAT64, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_FLOAT64, 0.5f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToClientDataDefinition(CLIENT_ENUM.PLACEHOLDER, SimConnect.SIMCONNECT_CLIENTDATAOFFSET_AUTO, SimConnect.SIMCONNECT_CLIENTDATATYPE_INT8, 0.5f, SimConnect.SIMCONNECT_UNUSED);
System.Console.Error.WriteLine($"Added double to client data def {simConnect.GetLastSentPacketID()}");

            simConnect.OnRecvClientData += GotSome;
            simConnect.RequestClientData(CLIENT_ENUM.PLACEHOLDER, CLIENT_ENUM.PLACEHOLDER, CLIENT_ENUM.PLACEHOLDER,
                SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET, SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);
        }

        private void GotSome(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
        {
//System.Console.Error.WriteLine($"Got me some client data, request ID {data.dwRequestID}; define ID {data.dwDefineID}; object {data.dwObjectID}");
            switch ((CLIENT_ENUM)data.dwRequestID)
            {
                case CLIENT_ENUM.PLACEHOLDER:
                    VSpeedsCallData callData = (VSpeedsCallData)data.dwData[0];
                    MaybeCall(callData);
                    break;
            }
        }

        bool wasAbove80 = false;
        bool wasAboveV1 = false;
        bool wasAboveVR = false;

        private void MaybeCall(VSpeedsCallData callData)
        {
System.Console.Error.WriteLine($"Airspeed {callData.airspeed} V1 {callData.v1} VR {callData.vr} phase {callData.phase}");
            setAndCallIfRequired(80, callData.airspeed, "eighty knots", ref wasAbove80);
            setAndCallIfRequired(callData.v1, callData.airspeed, "vee one", ref wasAboveV1);
            setAndCallIfRequired(callData.vr, callData.airspeed, "rotate", ref wasAboveVR);
        }

        private void setAndCallIfRequired(double calledSpeed, double actualSpeed, string call, ref bool wasAbove)
        {
            bool isAbove = calledSpeed > 0 && actualSpeed >= calledSpeed;
            if (isAbove && !wasAbove)
            {
                hubContext.Clients.All.Speak(call);
            }
            wasAbove = isAbove;
        }
    }
}
