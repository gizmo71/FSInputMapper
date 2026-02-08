using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

// Consider moving the ground vibes to the stick and using the throttle for speedbrakes in air instead
namespace Controlzmo.Systems.Controls.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct VibeData
    {
        [SimVar("GPS GROUND SPEED", "Knots", SIMCONNECT_DATATYPE.INT32, 2.0f)]
        public Int32 groundSpeed;
        [SimVar("SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onGround;
        [SimVar("ON ANY RUNWAY", "Bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 onAnyRunway;
    };

    [Component, RequiredArgsConstructor]
    public partial class GroundVibeListener : DataListener<VibeData>, IRequestDataOnOpen
    {
        private readonly UrsaMinorOutputs output;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, VibeData data)
        {
            double knots = Math.Min(200, data.groundSpeed);
            double percent = knots > 0 && data.onGround != 0 ? Double.Clamp(Math.Pow(knots / 20.0, 2), 1.0, 100.0) : 0;
            if (data.onAnyRunway != 0) percent /= 2.0; // Runways are smoother!
            output.SetVibrations((byte) percent);
        }
    }
}
