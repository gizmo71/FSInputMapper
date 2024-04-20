using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.FlightControlUnit
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuTopRightData
    {
        [SimVar("L:A32NX_TRK_FPA_MODE_ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isTrkFpaMode;
        [SimVar("L:I_FCU_TRACK_FPA_MODE", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isTrkFpaModeFenix;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class FcuTrackFpaToggled : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;
        public string SimEvent() => "A32NX.FCU_TRK_FPA_TOGGLE_PUSH";
        public string GetId() => "trkFpaToggled";

        public void SetInSim(ExtendedSimConnect simConnect, bool _)
        {
            if (simConnect.IsFenix)
                for (int i = 0; i < 2; ++i)
                    sender.Execute(simConnect, "(L:S_FCU_HDGVS_TRKFPA) ++ (>L:S_FCU_HDGVS_TRKFPA)");
            else
                simConnect.SendEvent(this);
        }

    }
}
