using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BrakesHotData
    {
        [SimVar("L:A32NX_BRAKES_HOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 areBrakesHot;
    };

    [Component]
    public class BrakesHot : DataListener<BrakesHotData>, IOnSimStarted
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public BrakesHot(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, BrakesHotData data)
        {
            if (data.areBrakesHot == 1)
                hubContext.Clients.All.Speak("Brakes hot");
        }
    }
}
