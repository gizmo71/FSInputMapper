using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com1RxToggleEvent : IEvent
    {
        public string SimEvent() => "COM1_RECEIVE_SELECT";
    }

    [Component]
    public class Com2RxToggleEvent : IEvent
    {
        public string SimEvent() => "COM2_RECEIVE_SELECT";
    }

    // Beware confusing names! VHF_L/C/R are actually COM1/2/3, and LVar COM_1/2/3 is Captain/Fo/overhead panel.

//TODO: support COM1 and split the structure apart.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Com2RxData
    {
        [SimVar("L:XMLVAR_COM_1_VHF_C_Switch_Down", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 isCaptainVhf2SwitchDown;
        [SimVar("L:XMLVAR_COM_2_VHF_C_Switch_Down", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 isFoVhf2SwitchDown;
        [SimVar("COM RECEIVE:2", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 isCom2Rx; // Not settable, done using an event.
    };

    [Component]
    public class Com2Rx : DataListener<Com2RxData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly Com2RxToggleEvent toggle;
        private readonly ILogger logger;

        public Com2Rx(IServiceProvider serviceProvider)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            toggle = serviceProvider.GetRequiredService<Com2RxToggleEvent>();
            logger = serviceProvider.GetRequiredService<ILogger<Com2Rx>>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, Com2RxData data)
        {
            bool isCom2Receiving = data.isCaptainVhf2SwitchDown == 1 || data.isFoVhf2SwitchDown == 1;
logger.LogCritical($"LVars {data.isCaptainVhf2SwitchDown}/{data.isFoVhf2SwitchDown} say {isCom2Receiving} and AVar says {data.isCom2Rx}");
            hub.Clients.All.SetFromSim(GetId(), isCom2Receiving);
        }

        public string GetId() => "com2rx";

        public void SetInSim(ExtendedSimConnect simConnect, bool isReceiving)
        {
            UInt32 newSetting = isReceiving ? 1u : 0u;
            simConnect.SendDataOnSimObject(new Com2RxData() {
                isCaptainVhf2SwitchDown = newSetting, isFoVhf2SwitchDown = newSetting
            });
            simConnect.SendEvent(toggle, newSetting);
         }
    }
}
