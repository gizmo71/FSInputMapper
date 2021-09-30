using Controlzmo.Serial;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.EfisControlPanel
{
    //TODO 1 or 2 (>K:TOGGLE_FLIGHT_DIRECTOR)

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FlightDirectorData
    {
        [SimVar("AUTOPILOT FLIGHT DIRECTOR ACTIVE:1", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fdActive1;
        [SimVar("AUTOPILOT FLIGHT DIRECTOR ACTIVE:2", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fdActive2;
    };

    [Component]
    public class FlightDirectorListener : DataListener<FlightDirectorData>, IRequestDataOnOpen
    {
        private readonly ILogger logging;
        private readonly SerialPico serial;

        public FlightDirectorListener(IServiceProvider sp)
        {
            logging = sp.GetRequiredService<ILogger<FlightDirectorListener>>();
            serial = sp.GetRequiredService<SerialPico>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FlightDirectorData data)
            => logging.LogError($"FdActive1={data.fdActive1} FdActive2={data.fdActive2}");
    }
}
