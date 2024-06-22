using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CockpitDoorData
    {
        [SimVar("L:A32NX_COCKPIT_DOOR_LOCKED", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 a32nxCockpitDoorLocked;
        [SimVar("L:B_DOORS_COCKPIT_LOCKED", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenixCockpitDoorLocked; // Not sure this does anything...
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
            /*TODO: if Fenix, set L:S_PED_COCKPIT_DOOR to 0 for lock or 2 for unlock. Doesn't work if cabin not enabled.
              If INI, set L:INI_COCKPIT_DOOR_LOCK_SWITCH to 2 for lock or 0 for unlock. Not sure it does anything...
              Both return to 1 in the central "norm" position. */
            simConnect.SendDataOnSimObject(new CockpitDoorData() { a32nxCockpitDoorLocked = isLocked ? 1 : 0 });
        }
        
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, CockpitDoorData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.a32nxCockpitDoorLocked == 1);
        }
    }
}
