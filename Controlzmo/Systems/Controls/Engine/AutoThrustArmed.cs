using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Controls.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AutoThrustArmedData
    {
        [SimVar("L:AS01B_AUTO_THROTTLE_ARM_STATE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 state;
    }

    [Component, RequiredArgsConstructor]
    public partial class AutoThrustArmed : DataListener<AutoThrustArmedData>, IRequestDataOnOpen
    {
        private bool _isArmed = false;
        internal bool IsArmed { get => _isArmed;}
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;
        public override void Process(ExtendedSimConnect simConnect, AutoThrustArmedData data) => _isArmed = data.state == 1;
    }
}
