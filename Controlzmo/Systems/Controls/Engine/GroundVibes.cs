using Controlzmo.GameControllers;
using Controlzmo.Systems.PilotMonitoring;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

// Consider moving the ground vibes to the stick and using the throttle for speedbrakes in air instead
namespace Controlzmo.Systems.Controls.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GroundVibeData
    {
        [SimVar("GPS GROUND SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 2.5f)]
        public Int32 groundSpeed;
        [SimVar("ON ANY RUNWAY", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onAnyRunway;
    };

    [Component, RequiredArgsConstructor]
    public partial class GroundVibeListener : DataListener<GroundVibeData>, IOnGroundHandler
    {
        private readonly UrsaMinorOutputs output;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            var period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            if (!isOnGround) output.SetVibrations(0);
        }

        public override void Process(ExtendedSimConnect simConnect, GroundVibeData data)
        {
            int percent = Math.Min(200, data.groundSpeed);
            if (data.onAnyRunway != 0) percent /= 2; // Runways are smoother!
            output.SetVibrations((byte) percent);
        }
    }
}
