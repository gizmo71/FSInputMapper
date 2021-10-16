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

/* xpndr mode (settable `TRANSPONDER STATE:1` 1 Stby or Auto, 3 on without Alt Rptg, 4 on with Alt Rptg)
  1 is standby or auto, 3/4 appear to be "On" in sim with alt rptg in off/on respectively, but if one, doesn't report the other when alt rptg is switched :-(
  In auto, is unsettable. It's likely that the value changes once airbourne if in standby.
  Setting to 1 from On always turns it to Standby, regardless of Alt Rptg.
  If Alt Rptg is On, only 4 will set it to On.
  If Alt Rptg is Off, only 3 will set it to On.
* Alt Rptg L:A32NX_SWITCH_ATC_ALT - can be set to 0 off, 1 auto, or 2 on.
* Ident? (local event `A320_Neo_ATC_BTN_IDENT`?) */
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
}
