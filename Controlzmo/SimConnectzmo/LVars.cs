using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.SimConnectzmo
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LVarDataRequest
    {
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 52)]
        public string name;
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.I4)]
        public Int32 milliseconds; // One of 0, 167, 1000, 4000
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.R8)]
        public double value;
    };

    [Component]
    public class LVarRequester : DataSender<LVarDataRequest>, IClientData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private const string ClientDataName = "Controlzmo.LVarRequest";

        public string GetClientDataName() => ClientDataName;

        public void Request(ExtendedSimConnect simConnect, string name, int milliseconds, double value)
        {
            var data = new LVarDataRequest { name = name, milliseconds = milliseconds, value = value };
            simConnect.SendDataOnSimObject(data);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LVarDataResponse
    {
        /*	char varName[52];
            int32_t id; // One of LVAR_POLL_* on input; -1 on output if variable not defined.
            double value; // On request, sets the "current" value.
        SimConnect_MapClientDataNameToID(g_hSimConnect, "Controlzmo.LVarRequest", CLIENT_DATA_ID_LVAR_REQUEST);*/
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 52)]
        public string name;
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.I4)]
        public Int32 id;
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.R8)]
        public double value;
    };

    [Component]
    public class LVarListener : DataListener<LVarDataResponse>, IRequestDataOnOpen, IClientData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] // Why is this needed and how is it used?
        private const string ClientDataName = "Controlzmo.LVarResponse";

        private readonly ILogger<LVarListener> _logging;

        public LVarListener(ILogger<LVarListener> logging)
        {
            _logging = logging;
        }

        public string GetClientDataName() => ClientDataName;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => (SIMCONNECT_PERIOD)SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET;

        public override void Process(ExtendedSimConnect simConnect, LVarDataResponse data)
        {
            _logging.LogInformation($"LVar {data.name} ({data.id}) = {data.value}");
        }
    }

    //TODO: new classes for listening for specific LVars, plugged in to request and deal with responses.
}
