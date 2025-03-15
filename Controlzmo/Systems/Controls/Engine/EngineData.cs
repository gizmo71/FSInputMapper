using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.Runtime.InteropServices;
using System;

namespace Controlzmo.Systems.Controls.Engine
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineData
    {
        [SimVar("NUMBER OF ENGINES", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 count;
        [SimVar("THROTTLE LOWER LIMIT", "Number", SIMCONNECT_DATATYPE.FLOAT32, 0.5f)]
        public float throttleLowerLimit;
    };

    [Component, RequiredArgsConstructor]
    public partial class EngineDataListener : DataListener<EngineData>, IRequestDataOnOpen
    {
        [Property]
        private int _numberOfEngines;
        [Property]
        private float _throttleLowerLimit;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, EngineData data)
        {
            _numberOfEngines = data.count;
            _throttleLowerLimit = data.throttleLowerLimit;
        }
    }
}
