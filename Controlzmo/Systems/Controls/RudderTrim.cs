using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Controls
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RudderTrimData
    {
        [SimVar("RUDDER TRIM PCT", "percent", SIMCONNECT_DATATYPE.FLOAT32, 0.00005f)]
        public float trimPct;
        [SimVar("L:RUDDER_TRIM_TARGET_ANGLE", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.00005f)]
        public float iniBuilds;
        [SimVar("L:A32NX_RUDDER_TRIM_ACTUAL_POSITION", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.005f)]
        public float a380xTrim;
        [SimVar("L:A32NX_HYD_RUDDER_TRIM_FEEDBACK_ANGLE", "number", SIMCONNECT_DATATYPE.FLOAT32, 0.005f)]
        public float a32nxTrim;
        [SimVar("L:N_FC_RUDDER_TRIM_DECIMAL", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenixDecaUnits;
    };

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimDisplay : DataListener<RudderTrimData>, IRequestDataOnOpen
    {
        private readonly UrsaMinorOutputs output;
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, RudderTrimData data)
        {
//Console.Write($"trim percent {data.fenixDecaUnits} FBW {data.a32nxTrim} ini {data.iniBuilds} pct {data.trimPct}");
            if (simConnect.IsA380X)
                data.fenixDecaUnits = (Int32) (Math.Round(data.a380xTrim * -10.0, MidpointRounding.AwayFromZero));
            else if (simConnect.IsA32NX)
                data.fenixDecaUnits = (Int32) (Math.Round(data.a32nxTrim * 10.0, MidpointRounding.AwayFromZero));
            else if (simConnect.IsIniBuilds)
                data.fenixDecaUnits = (Int32) (Math.Round(data.iniBuilds * 10.0, MidpointRounding.ToZero));
            else if (!simConnect.IsFenix)
                data.fenixDecaUnits = (Int32) (data.trimPct * 10);
//Console.WriteLine($" -> {data.fenixDecaUnits}");
            output.SetTrimDisplay(data.fenixDecaUnits);
        }
    }

    [Component]
    public class RudderTrimResetEvent : IEvent { public string SimEvent() => "RUDDER_TRIM_RESET"; }

    [Component, RequiredArgsConstructor]
    public partial class RudderTrimReset : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly RudderTrimResetEvent _event;
        public int GetButton() => UrsaMinorThrottle.BUTTON_RUDDER_TRIM_RESET;
        public virtual void OnPress(ExtendedSimConnect sc) => sc.SendEvent(_event);
    }
}
