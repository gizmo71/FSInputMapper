using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct OnGroundAndGearData
    {
        [SimVar("GEAR HANDLE POSITION", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 gear1IfDown;
        [SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onGround;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class GearListener : DataListener<OnGroundAndGearData>, IRequestDataOnOpen
    {
        private readonly PositiveClimbListener positiveClimbListener;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, OnGroundAndGearData data)
        {
            var period = data.gear1IfDown == 1 && data.onGround == 0 ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(positiveClimbListener, period);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ClimbRateData
    {
        [SimVar("VERTICAL SPEED", "Feet per minute", SIMCONNECT_DATATYPE.INT32, 100.0f)]
        public Int32 feetPerMinute;
    };

    [Component]
    public class PositiveClimbListener : DataListener<ClimbRateData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public PositiveClimbListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public override void Process(ExtendedSimConnect simConnect, ClimbRateData data)
        {
            if (data.feetPerMinute > 500)
            {
                hubContext.Clients.All.Speak("positive climb");
                simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.NEVER);
            }
        }
    }
}
