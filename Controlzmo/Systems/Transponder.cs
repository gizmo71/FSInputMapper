﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Transactions;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
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
    public class SquawkIdent : AbstractButton
    {
        private readonly JetBridgeSender sender;
        public SquawkIdent(SquawkIdentEvent squawkIdentEvent, JetBridgeSender sender) : base(squawkIdentEvent)
        {
            this.sender = sender;
        }

        public override string GetId() => "squawkIdent";

        public override void SetInSim(ExtendedSimConnect simConnect, object? value)
        {
            if (simConnect.IsFenix)
            {
                for (int i = 1; i >= 0; --i)
                    sender.Execute(simConnect, $"{i} (>L:S_XPDR_IDENT)");
            }
            else
                base.SetInSim(simConnect, value);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AltRptgData
    {
        [SimVar("L:A32NX_SWITCH_ATC_ALT", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isAltRptgOn;
        [SimVar("L:S_XPDR_ALTREPORTING", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isAltRptgOnFenix;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class AltRptg : DataListener<AltRptgData>, IRequestDataOnOpen, ISettable<bool?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "altRptg";

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, AltRptgData data)
        {
            hub.Clients.All.SetFromSim(GetId(), (simConnect.IsFenix ? data.isAltRptgOnFenix : data.isAltRptgOn) == 1);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? isOn)
        {
            var value = isOn == true ? 1 : 0;
            simConnect.SendDataOnSimObject(new AltRptgData() { isAltRptgOn = value, isAltRptgOnFenix = value });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TcasModeData
    {
        [SimVar("L:A32NX_SWITCH_TCAS_Position", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 position;
        [SimVar("L:S_XPDR_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 positionFenix;
    };

    [Component]
    public class TcasMode : DataListener<TcasModeData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TcasMode(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, TcasModeData data)
        {
            hub.Clients.All.SetFromSim(GetId(), simConnect.IsFenix ? data.positionFenix : data.position);
        }

        public string GetId() => "tcasMode";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var code = Int16.Parse(posString!);
            simConnect.SendDataOnSimObject(new TcasModeData() { position = code, positionFenix = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TcasTrafficModeData
    {
        [SimVar("L:A32NX_SWITCH_TCAS_Traffic_Position", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 position;
        [SimVar("L:S_TCAS_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 positionFenix;
    };

    [Component]
    public class TcasTraffic : DataListener<TcasTrafficModeData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TcasTraffic(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, TcasTrafficModeData data)
        {
            hub.Clients.All.SetFromSim(GetId(), simConnect.IsFenix ? data.positionFenix : data.position);
        }

        public string GetId() => "tcasTraffic";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var code = Int16.Parse(posString!);
            simConnect.SendDataOnSimObject(new TcasTrafficModeData() { position = code, positionFenix = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TransponderModeData
    {
        [SimVar("L:A32NX_TRANSPONDER_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 xpndrMode;
        [SimVar("L:S_XPDR_OPERATION", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 xpndrModeFenix;
    };

    [Component]
    public class TransponderMode : DataListener<TransponderModeData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public TransponderMode(IServiceProvider sp)
        {
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, TransponderModeData data)
        {
            hub.Clients.All.SetFromSim(GetId(), simConnect.IsFenix ? data.xpndrModeFenix : data.xpndrMode);
        }

        public string GetId() => "transponderMode";

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var code = Int16.Parse(posString!);
            simConnect.SendDataOnSimObject(new TransponderModeData() { xpndrMode = code, xpndrModeFenix = code });
        }
    }
}
