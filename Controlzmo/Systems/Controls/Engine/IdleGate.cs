using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Controls.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IdleGateData
    {
        [SimVar("L:MSATR_ENG_IDLE_GATE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isFlightIdle;
    };

    [Component, RequiredArgsConstructor]
    public partial class IdleGate : DataListener<IdleGateData>, IRequestDataOnOpen
    {
        private const double GROUND_IDLE_MAN_START = -0.480;
        private const double FLIGHT_IDLE_MAN_START = -0.362;
        private const double MAN_END = -0.015;
        private const double GROUND_IDLE_RANGE = MAN_END - GROUND_IDLE_MAN_START;
        private const double FLIGHT_IDLE_RANGE = MAN_END - FLIGHT_IDLE_MAN_START;

        private bool _isFlightIdle;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, IdleGateData data) => _isFlightIdle = data.isFlightIdle == 1 && simConnect.IsAtr;

        internal void MaybeMap(ref double normalised)
        {
            if (_isFlightIdle && normalised >= GROUND_IDLE_MAN_START && normalised <= MAN_END)
            {
                double withinRange = (normalised - GROUND_IDLE_MAN_START) / GROUND_IDLE_RANGE;
                normalised = withinRange * FLIGHT_IDLE_RANGE + FLIGHT_IDLE_MAN_START;
            }
        }
    }
}
