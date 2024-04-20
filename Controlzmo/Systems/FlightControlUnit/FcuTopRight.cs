using Controlzmo.Serial;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    [RequiredArgsConstructor]
    public partial class FcuDisplayTopRight : DataListener<FcuTopRightData>, IRequestDataOnOpen
    {
        private readonly SerialPico serial;

        public bool isTrkFpa = false;
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuTopRightData data)
        {
            isTrkFpa = (simConnect.IsFenix ? data.isTrkFpaModeFenix : data.isTrkFpaMode) == 0;
//TODO: notify everyone who wants it... perhaps triggering them to get their own data again...
            var line1 = "ALT \x4LVL/CH\x5 " + (isTrkFpa ? "V/S" : "FPA");
            serial.SendLine($"fcuTR={line1}");
        }
    }
}
