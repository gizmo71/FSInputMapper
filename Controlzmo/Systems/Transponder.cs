﻿using System;
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
    public class AltRptg : ISettable<string?>
    {
        private readonly JetBridgeSender jetbridge;

        public AltRptg(IServiceProvider sp)
        {
            jetbridge = sp.GetRequiredService<JetBridgeSender>();
        }

        public string GetId() => "altRptg";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = posString switch { "on" => 2, "auto" => 1, _ => 0 };
            jetbridge.Execute(simConnect, $"{pos} (>L:A32NX_SWITCH_ATC_ALT)");
        }
    }

    [Component]
    public class TcasMode : LVar, IOnSimStarted, ISettable<string?>
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
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        public string GetId() => "tcasMode";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:{LVarName()})");
        }
    }

    [Component]
    public class TcasTraffic : LVar, IOnSimStarted, ISettable<string?>
    {
        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TcasTraffic(IServiceProvider sp) : base(sp)
        {
            jetbridge = sp.GetRequiredService<JetBridgeSender>();
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            PropertyChanged += (object? _, PropertyChangedEventArgs e) => hub.Clients.All.SetFromSim(GetId(), Value);
        }

        protected override string LVarName() => "A32NX_SWITCH_TCAS_Traffic_Position";
        protected override int Milliseconds() => 4000;
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        public string GetId() => "tcasTraffic";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:{LVarName()})");
        }
    }

    [Component]
    public class TransponderMode : LVar, IOnSimStarted, ISettable<string?>
    {
        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TransponderMode(IServiceProvider sp) : base(sp)
        {
            jetbridge = sp.GetRequiredService<JetBridgeSender>();
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            PropertyChanged += (object? _, PropertyChangedEventArgs e) => hub.Clients.All.SetFromSim(GetId(), Value);
        }

        protected override string LVarName() => "A32NX_TRANSPONDER_MODE";
        protected override int Milliseconds() => 4000;
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        public string GetId() => "transponderMode";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:{LVarName()})");
        }
    }
}
