using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.Systems.Radar;
using Controlzmo.Systems.Transponder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LVarData
    {
/*	char varName[52];
    int32_t id; // One of LVAR_POLL_* on input; -1 on output if variable not defined.
    double value; // On request, sets the "current" value.
SimConnect_MapClientDataNameToID(g_hSimConnect, "Controlzmo.LVarRequest", CLIENT_DATA_ID_LVAR_REQUEST);
SimConnect_MapClientDataNameToID(g_hSimConnect, "", CLIENT_DATA_ID_LVAR_RESPONSE);*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 52)]
        [ClientVar(0.5f)]
        public string name;
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.I4)]
        public Int32 id;
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.R8)]
        public double value;
    };

    [Component]
    public class LVarListener : DataListener<LVarData>, IRequestDataOnOpen
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] // Why is this needed and how is it used?
        private const string ClientDataName = "Controlzmo.LVarResponse";

        public string GetClientDataName() => ClientDataName;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => (SIMCONNECT_PERIOD)SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET;

        public override void Process(ExtendedSimConnect simConnect, LVarData data)
        {
            System.Console.Error.WriteLine($"LVar {data.name} ({data.id}) = {data.value}");
        }
    }

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
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.I1)]
        public Byte tcasTraffic;
    };

    [Component]
    public class LocalVarsListener : DataListener<LocalVarsData>, IClientData, IRequestDataOnOpen
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] // Why is this needed and how is it used?
        private const string VSpeedsClientDataName = "Controlzmo.VSpeeds";

        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public LocalVarsData localVars;

        public LocalVarsListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public string GetClientDataName() => VSpeedsClientDataName;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => (SIMCONNECT_PERIOD)SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET;

        public override void Process(ExtendedSimConnect simConnect, LocalVarsData localVars)
        {
System.Console.Error.WriteLine($"LVars updated to autobrake {localVars.autobrake}; V1/VR {localVars.v1}/{localVars.vr}");
System.Console.Error.WriteLine($"radar/PWS {localVars.radar}/{localVars.pws}, TCAS/traffic {localVars.tcas}/{localVars.tcasTraffic}");
            this.localVars = localVars;
            hub.Clients.All.SetFromSim(RadarSys.id, localVars.radar);
            hub.Clients.All.SetFromSim(PredictiveWindshearSys.id, localVars.pws);
            hub.Clients.All.SetFromSim(TcasMode.id, localVars.tcas);
            hub.Clients.All.SetFromSim(TcasTraffic.id, localVars.tcasTraffic);
        }
    }
}
