﻿using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

/*
If this works on the phone, consider getting L:AIRLINER_V1_SPEED and L:AIRLINER_VR_SPEED and calling them.
Maybe 80 knots too, all using "AIRSPEED INDICATED" and something to detect takeoff mode.
Would have to have a WASM module to send client events we could RX.
On landing, would be good to detect spoilers popping, reversers, and decel light.
https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-simvars.md
https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-events.md
*/
namespace Controlzmo.Systems.PilotMonitoring
{
    internal enum CLIENT_ENUM { PLACEHOLDER = 123 }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct VSpeedsCallData
    {
        public double airspeed;
        public double v1;
        public double vr;
    };

    [Component]
    public class Bob
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] //TODO: is this needed?
        private const string VSpeedsClientDataName = "Controlzmo.VSpeeds";

        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public Bob(IServiceProvider serviceProvider)
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
                    VSpeedsCallData callData = (VSpeedsCallData)data.dwData[0];
                    MaybeCall(callData);
                    break;
            }
        }

        bool above80 = false;
        bool aboveV1 = false;
        bool aboveVR = false;

        private void MaybeCall(VSpeedsCallData callData)
        {
            System.Console.Error.WriteLine($"Airspeed {callData.airspeed} V1 {callData.v1} VR {callData.vr}");

            if (80.0 > 0 && callData.airspeed >= 80.0)
            {
                if (!above80)
                {
                    hubContext.Clients.All.Speak("eighty knots");
                    above80 = true;
                }
            }
            else
            {
                above80 = false;
            }

            if (callData.v1 > 0 && callData.airspeed >= callData.v1)
            {
                if (!aboveV1)
                {
                    hubContext.Clients.All.Speak("vee one");
                    aboveV1 = true;
                }
            }
            else
            {
                aboveV1 = false;
            }

            if (callData.vr > 0 && callData.airspeed >= callData.vr)
            {
                if (!aboveVR)
                {
                    hubContext.Clients.All.Speak("rotate");
                    aboveVR = true;
                }
            }
            else
            {
                aboveVR = false;
            }
        }
    }
}
