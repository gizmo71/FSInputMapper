using Lombok.NET;
using Controlzmo.Systems.JetBridge;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

// The iniBuilds sync stuff really only works when interacting via the cockpit GUI. :-(
namespace Controlzmo.Systems.EfisControlPanel
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IniBaroSyncData
    {
        [SimVar("L:XMLVAR_Baro1_Mode", "number", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public Int32 leftMode;
        [SimVar("L:XMLVAR_Baro2_Mode", "number", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public Int32 rightMode;
        [SimVar("L:XMLVAR_Baro3_Mode", "number", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public Int32 isisMode;
        [SimVar("L:XMLVar_Baro_Selector_HPA_1", "number", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public Int32 leftUnits;
        [SimVar("L:XMLVar_Baro_Selector_HPA_2", "number", SIMCONNECT_DATATYPE.INT32, 0.1f)]
        public Int32 rightUnits;
    };

    [Component, RequiredArgsConstructor]
    public partial class IniBaroSync : DataListener<IniBaroSyncData>, IRequestDataOnOpen
    {
        private readonly JetBridgeSender sender;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, IniBaroSyncData data)
        {
            // Do we need to avoid this for non-iniBuilds aircraft?
            if (data.isisMode != data.leftMode || data.rightMode != data.leftMode || data.rightUnits != data.leftUnits)
            {
                data.isisMode = data.rightMode = data.leftMode;
                data.rightUnits = data.leftUnits;
                simConnect.SendDataOnSimObject(data);
            }
        }
    }
}
