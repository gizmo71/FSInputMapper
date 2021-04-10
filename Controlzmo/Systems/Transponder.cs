using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

/* xpndr mode (settable `TRANSPONDER STATE:1` 1 Stby or Auto, 3 on without Alt Rptg, 4 on with Alt Rptg)
  1 is standby or auto, 3/4 appear to be "On" in sim with alt rptg in off/on respectively, but if one, doesn't report the other when alt rptg is switched :-(
  In auto, is unsettable. It's likely that the value changes once airbourne if in standby.
  Setting to 1 from On always turns it to Standby, regardless of Alt Rptg.
  If Alt Rptg is On, only 4 will set it to On.
  If Alt Rptg is Off, only 3 will set it to On.
* alt mode (`I:XMLVAR_ALT_MODE_REQUESTED`, `I:XMLVAR_Auto`, 1 if Auto, 0 if On or Stby - can't set at all)
* Ident? (local event `A320_Neo_ATC_BTN_IDENT`?) */
namespace Controlzmo.Systems.Transponder
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TransponderCodeData
    {
        [SimVar("TRANSPONDER CODE:1", "BCD16", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 bcd16;
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

            hub.Clients.All.SetFromSim(TransponderCode.id, $"{data.bcd16:X}");
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
    public class TcasMode : ISettable<string?>
    {
        public const string id = "tcasMode";

        private readonly JetBridgeSender jetbridge;

        public TcasMode(JetBridgeSender jetbridge)
        {
            this.jetbridge = jetbridge;
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:A32NX_SWITCH_TCAS_Position)");
        }
    }

    [Component]
    public class TcasTraffic : ISettable<string?>
    {
        public const string id = "tcasTraffic";

        private readonly JetBridgeSender jetbridge;

        public TcasTraffic(JetBridgeSender jetbridge)
        {
            this.jetbridge = jetbridge;
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:A32NX_SWITCH_TCAS_Traffic_Position)");
        }
    }
}
