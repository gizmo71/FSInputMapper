using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Radar
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RadarSysData
    {
        [SimVar("L:XMLVAR_A320_WeatherRadar_Sys", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 radarSys;
        [SimVar("L:S_WR_SYS", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 radarSysFenix;
    };

    [Component]
    public class RadarSys : DataListener<RadarSysData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public RadarSys(IServiceProvider serviceProvider)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public string GetId() => "radarSys";

        public override void Process(ExtendedSimConnect simConnect, RadarSysData data)
        {
            hub.Clients.All.SetFromSim(GetId(), simConnect.IsFenix ? data.radarSysFenix : data.radarSys);
        }

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var code = Int16.Parse(posString!);
            simConnect.SendDataOnSimObject(new RadarSysData() { radarSys = code, radarSysFenix = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PredictiveWindshearSysData
    {
        [SimVar("L:A32NX_SWITCH_RADAR_PWS_Position", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 pwsSwitch;
        [SimVar("L:S_WR_PRED_WS", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 pwsSwitchFenix;
    };

    [Component]
    public class PredictiveWindshearSys : DataListener<PredictiveWindshearSysData>, IRequestDataOnOpen, ISettable<bool?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public PredictiveWindshearSys(IServiceProvider serviceProvider)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public string GetId() => "predictiveWindshear";

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, PredictiveWindshearSysData data)
        {
            hub.Clients.All.SetFromSim(GetId(), (simConnect.IsFenix ? data.pwsSwitchFenix : data.pwsSwitch) == 1);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? isAuto)
        {
            Int32 value = isAuto == true ? 1 : 0;
            simConnect.SendDataOnSimObject(new PredictiveWindshearSysData() { pwsSwitch = value, pwsSwitchFenix = value });
        }
    }
}
