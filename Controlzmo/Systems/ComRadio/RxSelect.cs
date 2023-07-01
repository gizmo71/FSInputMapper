using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com2RxToggleEvent : IEvent
    {
        public string SimEvent() => "COM2_RECEIVE_SELECT";
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Com2RxData
    {
        [SimVar(Com2Rx.LVAR_NAME, "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public UInt32 isCom2VhfCSwitchDown;
    };

    [Component]
    public class Com2Rx : DataListener<Com2RxData>, IOnSimStarted, ISettable<bool>
    {
        internal const string LVAR_NAME = "L:XMLVAR_COM_2_VHF_C_Switch_Down";

        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly Com2RxToggleEvent toggle;

        public Com2Rx(IServiceProvider serviceProvider)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            toggle = serviceProvider.GetRequiredService<Com2RxToggleEvent>();
        }

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, Com2RxData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.isCom2VhfCSwitchDown == 1);
        }

        public string GetId() => "com2rx";

        public void SetInSim(ExtendedSimConnect simConnect, bool isReceiving)
        {
            UInt32 newSetting = isReceiving ? 1u : 0u;
            simConnect.SendDataOnSimObject(new Com2RxData() { isCom2VhfCSwitchDown = newSetting });
            simConnect.SendEvent(toggle, newSetting);
         }
    }
}
