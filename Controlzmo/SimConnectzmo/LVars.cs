using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.SimConnectzmo
{
    public abstract class LVar : INotifyPropertyChanged
    {
        private readonly LVarRequester requester;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual double? Value { get; set; }

        protected LVar(IServiceProvider serviceProvider)
        {
            requester = serviceProvider.GetRequiredService<LVarRequester>();
            requester.LVarUpdated += Update;
        }

        protected abstract string LVarName();
        protected abstract int Milliseconds();
        protected abstract double Default();

        public static implicit operator double?(LVar lVar) => lVar.Value;

        public void Request(ExtendedSimConnect simConnect)
        {
            requester.Request(simConnect, LVarName(), Milliseconds(), Default());
        }

        private void Update(string name, double? newValue)
        {
            if (name == LVarName() && Value != newValue)
            {
                Value = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }

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
    internal class LVarRequester : DataSender<LVarDataRequest>, IClientData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        private const string ClientDataName = "Controlzmo.LVarRequest";

        public delegate void LVarUpdatedHandler(string name, double? _);
        public LVarUpdatedHandler? LVarUpdated;

        public string GetClientDataName() => ClientDataName;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void Request(ExtendedSimConnect simConnect, string name, int milliseconds, double value)
        {
            if (milliseconds != 0 && milliseconds != 167 && milliseconds != 1000 && milliseconds != 4000)
                throw new ArgumentException($"{milliseconds}ms not supported as checking interval");
            var data = new LVarDataRequest { name = name, milliseconds = milliseconds, value = value };
            simConnect.SendDataOnSimObject(data);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LVarDataResponse
    {
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 52)]
        public string name;
        [ClientVar(0.5f)]
        [MarshalAs(UnmanagedType.I4)]
        public Int32 id;
        [ClientVar(0.000001f)]
        [MarshalAs(UnmanagedType.R8)]
        public double value;
    };

    [Component]
    internal class LVarListener : DataListener<LVarDataResponse>, IRequestDataOnOpen, IClientData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] // Why is this needed and how is it used?
        private const string ClientDataName = "Controlzmo.LVarResponse";

        private readonly ILogger<LVarListener> logging;
        private readonly LVarRequester requester;

        public LVarListener(ILogger<LVarListener> logging, LVarRequester requester)
        {
            this.logging = logging;
            this.requester = requester;
        }

        public string GetClientDataName() => ClientDataName;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => (SIMCONNECT_PERIOD)SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET;

        public override void Process(ExtendedSimConnect simConnect, LVarDataResponse data)
        {
            logging.LogInformation($"LVar {data.name} ({data.id}) = {data.value}");
            requester.LVarUpdated?.Invoke(data.name, data.id == -1 ? null : data.value);
        }
    }

    //TODO: new classes for listening for specific LVars, plugged in to request and deal with responses.
}
