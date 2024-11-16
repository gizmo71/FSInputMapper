using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Controlzmo.Hubs;

namespace Controlzmo.Systems.ComRadio
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ComTxData
    {
        [SimVar("COM TRANSMIT:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 com1Tx;
        [SimVar("COM TRANSMIT:2", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 com2Tx;
        [SimVar("COM TRANSMIT:3", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 com3Tx;
    };

    [Component]
    public class PilotTxSetEvent : IEvent
    {
        public string SimEvent() => "PILOT_TRANSMITTER_SET"; // Parameter: 0=COM1, 1=COM2, 2=COM3, 4=None
    }

    [Component, RequiredArgsConstructor]
    public partial class ComTx : DataListener<ComTxData>, IRequestDataOnOpen, ISettable<String>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly PilotTxSetEvent setEvent;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;
        public string GetId() => "comtx";

        public override void Process(ExtendedSimConnect simConnect, ComTxData data)
        {
            var channel = "0";
            if (data.com1Tx != 0) channel = "1";
            else if (data.com2Tx != 0) channel = "2";
            else if (data.com3Tx != 0) channel = "3";
            hub.Clients.All.SetFromSim(GetId(), channel);
        }

        public void SetInSim(ExtendedSimConnect simConnect, String? channel)
        {
            UInt32 value = 4;
            if (channel != "0") value = UInt32.Parse(channel!) - 1;
            simConnect.SendEvent(setEvent, value);
        }
    }
}
