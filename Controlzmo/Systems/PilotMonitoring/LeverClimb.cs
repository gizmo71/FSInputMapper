using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AutothrustModeMessageData
    {
//TODO: can/should we also check for a positive speed trend before announcing? Or only TLA is above CLB?
// "ACCELERATION BODY Z"? Or will just increasing the delay help, by spotting it if accidental during flight?
        [SimVar("L:A32NX_AUTOTHRUST_MODE_MESSAGE", "number", SIMCONNECT_DATATYPE.INT32, 1f)]
        public Int32 modeMessage;
        [SimVar("L:A32NX_AUTOTHRUST_STATUS", "number", SIMCONNECT_DATATYPE.INT32, 1f)]
        public Int32 status;
        [SimVar("ABSOLUTE TIME", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 1.0f)]
        public Double now; // Only has second granularity, despite apparent precision; stops for pauses
        [SimVar("ACCELERATION BODY Z", "feet per second squared", SIMCONNECT_DATATYPE.FLOAT64, 100.0f)]
        public Double accZ; // Or L:A32NX_FAC_1_SPEED_TREND in knots?
    };

    [Component, RequiredArgsConstructor]
    public partial class LeverClimb : DataListener<AutothrustModeMessageData>, IOnSimStarted
    {
        private readonly Speech speech;
        private readonly ILogger<LeverClimb> logging;

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        private Double? callAfter = null;
        private bool wantPositiveSpeedTrend = false;

        public override void Process(ExtendedSimConnect simConnect, AutothrustModeMessageData data)
        {
//logging.LogInformation($"Level climb monitor {data.now} mode {data.modeMessage} accZ {data.accZ} status {data.status}");
            if (data.modeMessage == 3) {
                if (callAfter == null)
                {
                    wantPositiveSpeedTrend = data.status == 1; // Armed
                    callAfter = data.now + 2.9;
                }
                else if (data.now > callAfter && (!wantPositiveSpeedTrend || data.accZ > 1))
                {
                    callAfter = Double.PositiveInfinity;
                    speech.Say("Lever climb?");
                }
            }
            else
            {
                callAfter = null;
            }
        }
    }
}
