using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

// Ident? (local event `A320_Neo_ATC_BTN_IDENT`?)
// L:A32NX_TRANSPONDER_SYSTEM 0=XPNDR1, 1=XPNDR2
namespace Controlzmo.Systems.Transponder
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TransponderCodeData
    {
        [SimVar("TRANSPONDER CODE:1", "BCO16", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 binaryCodedOctal;
    };

    [Component]
    public class TransponderCodeListener : DataListener<TransponderCodeData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TransponderCodeListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, TransponderCodeData data)
        {
            hub.Clients.All.SetFromSim(TransponderCode.id, $"{data.binaryCodedOctal:X4}");
        }
    }

    [Component]
    public class SetTransponderCodeEvent : IEvent
    {
        public string SimEvent() => "XPNDR_SET";
    }

    [Component]
    public class TransponderCode : ISettable<string?>
    {
        internal const string id = "xpndrCode";

        private readonly SetTransponderCodeEvent setEvent;

        public TransponderCode(SetTransponderCodeEvent setEvent)
        {
            this.setEvent = setEvent;
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? code)
        {
            simConnect.SendEvent(setEvent, Convert.ToUInt32(code, 16));
        }
    }

    [Component]
    public class SquawkIdentEvent : IEvent
    {
        public string SimEvent() => "XPNDR_IDENT_ON";
    }

    [Component]
    public class SquawkIdent1Swap : AbstractButton
    {
        public SquawkIdent1Swap(SquawkIdentEvent squawkIdentEvent) : base(squawkIdentEvent) { }
        public override string GetId() => "squawkIdent";
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AltRptgData
    {
        [SimVar("L:A32NX_SWITCH_ATC_ALT", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isAltRptgOn;
    };

    [Component]
    public class AltRptg : DataListener<AltRptgData>, ISettable<bool?>, IOnSimConnection
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public AltRptg(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public string GetId() => "altRptg";

        public void OnConnection(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, AltRptgData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.isAltRptgOn == 1);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? isOn)
        {
            simConnect.SendDataOnSimObject(new AltRptgData() { isAltRptgOn = isOn == true ? 1 : 0 });
        }
    }

    [Component]
    public class TcasMode : LVar, IOnSimConnection, ISettable<string?>
    {
        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TcasMode(IServiceProvider sp) : base(sp)
        {
            jetbridge = sp.GetRequiredService<JetBridgeSender>();
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            PropertyChanged += (object? _, PropertyChangedEventArgs e) => hub.Clients.All.SetFromSim(GetId(), Value);
        }

        protected override string LVarName() => "A32NX_SWITCH_TCAS_Position";
        protected override int Milliseconds() => 4000;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        public string GetId() => "tcasMode";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:{LVarName()})");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TcasTrafficModeData
    {
        [SimVar("L:A32NX_SWITCH_TCAS_Traffic_Position", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 position;
    };

    [Component]
    public class TcasTraffic : DataListener<TcasTrafficModeData>, IOnSimStarted, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TcasTraffic(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, TcasTrafficModeData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.position);
        }

        public string GetId() => "tcasTraffic";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            simConnect.SendDataOnSimObject(new TcasTrafficModeData() { position = Int16.Parse(posString!) });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TransponderModeData
    {
        [SimVar("L:A32NX_TRANSPONDER_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 xpndrMode;
    };

    [Component]
    public class TransponderMode : DataListener<TransponderModeData>, IOnSimStarted, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TransponderMode(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, TransponderModeData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.xpndrMode);
        }

        public string GetId() => "transponderMode";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            simConnect.SendDataOnSimObject(new TransponderModeData() { xpndrMode = Int16.Parse(posString!) });
        }
    }
}
