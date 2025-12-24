using System;
using System.Runtime.InteropServices;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BrakesHotData
    {
        [SimVar("L:A32NX_BRAKES_HOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 areBrakesHot;
    };

    [Component, RequiredArgsConstructor]
    public partial class BrakesHot : DataListener<BrakesHotData>, IOnSimStarted
    {
        private readonly Speech speech;

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, BrakesHotData data)
        {
            if (data.areBrakesHot == 1)
                speech.Say("Brakes hot");
        }
    }
}
