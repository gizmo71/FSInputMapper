using System.Runtime.InteropServices;
using Controlzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BaroData
    {
        [SimVar("SEA LEVEL PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float seaLevelPressureMB;
        [SimVar("KOHLSMAN SETTING MB", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB;
        [SimVar("KOHLSMAN SETTING MB:2", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB2;
        [SimVar("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanHg;
    };

    [Component]
    public class BaroListener : DataListener<BaroData>, IRequestDataOnOpen
    {
        private readonly IHubContext<LightHub, ILightHub> hub;

        public BaroListener(IHubContext<LightHub, ILightHub> hub)
        {
            this.hub = hub;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(ExtendedSimConnect simConnect, BaroData data)
        {
            hub.Clients.All.ShowMessage($"MSL {data.seaLevelPressureMB}MB\nKohlsman {data.kohlsmanMB}MB"
                + $" (second {data.kohlsmanMB2}MB) {data.kohlsmanHg.ToString("N2")}Hg");
        }
    }
}
