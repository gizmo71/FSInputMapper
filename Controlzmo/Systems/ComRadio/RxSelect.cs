using Controlzmo.SimConnectzmo;
using SimConnectzmo;
using System;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Systems.ComRadio
{
    [Component]
    public class Com2RxToggleEvent : IEvent
    {
        public string SimEvent() => "COM2_RECEIVE_SELECT";
    }

    [Component]
    public class Com2Rx : LVar, IOnSimConnection, ISettable<bool>
    {
        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly Com2RxToggleEvent toggle;

        public Com2Rx(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            toggle = serviceProvider.GetRequiredService<Com2RxToggleEvent>();
        }

        protected override string LVarName() => "XMLVAR_COM_2_VHF_C_Switch_Down";
        protected override int Milliseconds() => 4000;
        protected override double Default() => -1.0;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        public string GetId() => "com2rx";

        protected override double? Value { set => hub.Clients.All.SetFromSim(GetId(), (base.Value = value) == 1.0); }

        public void SetInSim(ExtendedSimConnect simConnect, bool isReceiving)
        {
            var newSetting = isReceiving ? 1u : 0u;
            jetbridge.Execute(simConnect, $"{newSetting} (>L:{LVarName()})");
            simConnect.SendEvent(toggle, newSetting);
        }
    }
}
