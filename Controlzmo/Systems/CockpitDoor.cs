using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.Systems.Autothrust;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CockpitDoorData
    {
        [SimVar("L:A32NX_COCKPIT_DOOR_LOCKED", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxCockpitDoorLocked;
    };

    [Component]
    public class CockpitDoor : DataListener<CockpitDoorData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public CockpitDoor(IServiceProvider serviceProvider)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public string GetId() => "cockpitDoor";

        public void SetInSim(ExtendedSimConnect simConnect, bool isLocked)
        {
            simConnect.SendDataOnSimObject(new CockpitDoorData() { a32nxCockpitDoorLocked = isLocked ? 1 : 0 });
        }
        
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, CockpitDoorData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.a32nxCockpitDoorLocked == 1);
        }
    }
}
