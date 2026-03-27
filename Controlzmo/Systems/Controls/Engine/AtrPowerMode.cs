using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Controls.Engine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AtrPowerModeData
    {
        [SimVar("L:MSATR_ENG_PWRMGT_SYNC", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 isSync;
        [SimVar("L:MSATR_ENG_PWRMGT_1", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 mode1;
        [SimVar("L:MSATR_ENG_PWRMGT_2", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 mode2;
    };

    [Component, RequiredArgsConstructor]
    public partial class AtrPowerMode : DataListener<AtrPowerModeData>, IRequestDataOnOpen
    {
        private Int32 current = -1;

        internal void Manipulate(ExtendedSimConnect sc, int delta)
        {
            // 0=TO 1=MCT 2=CLB 3=CRZ
            // Note that MSATR_ENG_PWRMGT_SYNC must be 0 otherwise setting _1 or _2 will only work momentarily!
            // The input events DO NOT WORK for this :-(
            current = Math.Max(Math.Min(current + delta, 3), 0);
            var data = new AtrPowerModeData() { isSync = 0, mode1 = current, mode2 = current };
            sc.SendDataOnSimObject(data);
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, AtrPowerModeData data)
        {
            if (data.mode2 != (current = data.mode1))
                Manipulate(simConnect, 0);
        }
    }
}
