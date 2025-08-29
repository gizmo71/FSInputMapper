using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
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

//TODO: support COM1
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Com2RxData
    {
        [SimVar("COM RECEIVE:2", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 isCom2Rx; // Not settable, done using an event.
    };

    [Component]
    public class Com2RxListener : DataListener<Com2RxData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly string uiId;

        public Com2RxListener(IServiceProvider serviceProvider)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            uiId = serviceProvider.GetRequiredService<Com2Rx>().GetId();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, Com2RxData data)
        {
            hub.Clients.All.SetFromSim(uiId, data.isCom2Rx);
        }
    };

    // Beware confusing names! VHF_L/C/R are actually COM1/2/3, and LVar COM_1/2/3 is Captain/Fo/overhead panel.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Com2RxLVars
    {
        [SimVar("L:XMLVAR_COM_1_VHF_C_Switch_Down", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 isCaptainVhf2SwitchDown;
        [SimVar("L:XMLVAR_COM_2_VHF_C_Switch_Down", "bool", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 isFoVhf2SwitchDown;
    };

    [Component, RequiredArgsConstructor]
    public partial class Com2Rx : IData<Com2RxLVars>, ISettable<bool>
    {
        private readonly Com2RxToggleEvent toggle;
        private readonly InputEvents inputEvents;

        public string GetId() => "com2rx";

        public void SetInSim(ExtendedSimConnect simConnect, bool isReceiving)
        {
            UInt32 newSetting = isReceiving ? 1u : 0u;

            if (simConnect.IsIni330)
                inputEvents.Send(simConnect, "AIRLINER_CPT_VHF2_VOL_BUTTON", (double) newSetting);
            else if (simConnect.IsIni321)
                inputEvents.Send(simConnect, "AIRLINER_ACP_CPT_VHF2_VOL_BUTTON", (double) newSetting);
            else
            {
                simConnect.SendDataOnSimObject(new Com2RxLVars() {
                    isCaptainVhf2SwitchDown = newSetting, isFoVhf2SwitchDown = newSetting
                });
                simConnect.SendEvent(toggle, newSetting);
            }
         }
    }
}
