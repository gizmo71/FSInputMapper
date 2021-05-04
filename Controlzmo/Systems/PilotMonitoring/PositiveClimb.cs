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
    public struct GearData
    {
        [SimVar("GEAR HANDLE POSITION", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 gear1IfDown;
    };

    [Component]
    public class GearListener : DataListener<GearData>, IRequestDataOnOpen
    {
        private readonly PositiveClimbListener positiveClimbListener;

        public GearListener(PositiveClimbListener positiveClimbListener)
        {
            this.positiveClimbListener = positiveClimbListener;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, GearData data)
        {
            positiveClimbListener.SetIsLIstening(simConnect, data.gear1IfDown == 1);
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

        internal void SetIsLIstening(ExtendedSimConnect simConnect, bool IsListening)
        {
            SIMCONNECT_PERIOD period = IsListening ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
        }

        public override void Process(ExtendedSimConnect simConnect, ClimbRateData data)
        {
            if (data.feetPerMinute > 500)
            {
                hubContext.Clients.All.Speak("positive climb");
                SetIsLIstening(simConnect, false);
            }
        }
    }
}
