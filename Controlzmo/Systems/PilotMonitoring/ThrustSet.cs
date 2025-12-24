using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ThrustSetData
    {
        [SimVar("L:A32NX_AUTOTHRUST_TLA_N1:1", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.1f)]
        public Double tla1;
        [SimVar("L:A32NX_AUTOTHRUST_TLA_N1:2", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.1f)]
        public Double tla2;
        [SimVar("L:A32NX_ENGINE_N1:1", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.1f)]
        public Double engine1N1;
        [SimVar("L:A32NX_ENGINE_N1:2", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.1f)]
        public Double engine2N1;
        [SimVar("L:A32NX_AUTOTHRUST_MODE", "number", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 autothrustMode;
    };

    [Component, RequiredArgsConstructor]
    public partial class ThrustSetListener : DataListener<ThrustSetData>, IOnGroundHandler
    {
        private readonly Speech speech;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            simConnect.RequestDataOnSimObject(this, isOnGround ? SIMCONNECT_CLIENT_DATA_PERIOD.SECOND : SIMCONNECT_CLIENT_DATA_PERIOD.NEVER);
        }

        private Boolean isCalled = false;

        public override void Process(ExtendedSimConnect simConnect, ThrustSetData data)
        {
            if (IsTakeOffPower(data))
            {
                if (!isCalled && IsSet(data.engine1N1, data.tla1) && IsSet(data.engine2N1, data.tla2))
                {
                    speech.Say($"thrust set");
                    isCalled = true;
                }
            }
            else
                isCalled = false;
        }

        private const Int32 TOGA = 1;
        private const Int32 FLEX = 3;
        private static Boolean IsTakeOffPower(ThrustSetData data) => data.autothrustMode == TOGA || data.autothrustMode == FLEX;

        private static Boolean IsSet(Double engine, Double lever) => lever > 75.0 && engine >= lever - 0.1;
    }
}
