using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FuelLogData
    {
        [SimVar("L:A32NX_EFIS_L_TO_WPT_IDENT_0", "number", SIMCONNECT_DATATYPE.INT64, 1f)]
        public Int64 ident0;
        [SimVar("L:A32NX_EFIS_L_TO_WPT_IDENT_1", "number", SIMCONNECT_DATATYPE.INT64, 1f)]
        public Int64 ident1;
        [SimVar("L:A32NX_FUEL_USED:1", "number", SIMCONNECT_DATATYPE.FLOAT64, 50f)]
        public Double kgUsed1;
        [SimVar("L:A32NX_FUEL_USED:2", "number", SIMCONNECT_DATATYPE.FLOAT64, 50f)]
        public Double kgUsed2;
        [SimVar("FUEL TOTAL QUANTITY WEIGHT", "kg", SIMCONNECT_DATATYPE.FLOAT64, 50.0f)]
        public Double kgOnBoard;
    };

    [Component]
    [RequiredArgsConstructor]
    public partial class FuelLogListener : DataListener<FuelLogData>, IOnGroundHandler
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            simConnect.RequestDataOnSimObject(this, isOnGround ? SIMCONNECT_CLIENT_DATA_PERIOD.NEVER : SIMCONNECT_CLIENT_DATA_PERIOD.SECOND);
            nextWaypoint = "";
        }

        private String nextWaypoint = "";
        private String[] log = new string[] { "", "", "", "", "" };

        public override void Process(ExtendedSimConnect simConnect, FuelLogData data)
        {
            String ident = unpack(new Int64[] { data.ident0, data.ident1 });
            if (ident == nextWaypoint)
                return;
            var lastWaypoint = nextWaypoint;
            nextWaypoint = ident;
            if (lastWaypoint != "") {
                log[0] = log[1];
                log[1] = log[2];
                log[2] = log[3];
                log[3] = log[4];
                log[4] = $"At {lastWaypoint}: remaining {tons(data.kgOnBoard)}, used {tons(data.kgUsed1 + data.kgUsed2)}";
                hubContext.Clients.All.SetFromSim("fuelLog", String.Join('\n', log));
            }
        }

        private String tons(Double kg)
        {
            return $"{kg / 1000.0:F1}"; //:9.9
        }

        // https://github.com/flybywiresim/a32nx/blob/37e1e98029c0379493abb06da45d8de4378497b6/fbw-a32nx/src/systems/shared/src/simvar.ts#L8
        private String unpack(Int64[] values)
        {
            String ret = "";
            for (int i = 0; i < values.Length * 8; ++i)
            {
                var word = i / 8; // In TypeScript: Math.floor(i / 8)
                var chr = i % 8;
                var code = values[word] >> (chr * 6) & 0x3f;
                if (code > 0)
                    ret += (char) (code + 31);
            }
            return ret;
        }

    }
}
