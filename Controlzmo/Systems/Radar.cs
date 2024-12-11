using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
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
        [SimVar("L:INI_WX_SYS_SWITCH", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 radarSysIni;
    };

    [Component, RequiredArgsConstructor]
    public partial class RadarSys : DataListener<RadarSysData>, IRequestDataOnOpen, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly JetBridgeSender sender;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public string GetId() => "radarSys";

        public override void Process(ExtendedSimConnect simConnect, RadarSysData data)
        {
            if (simConnect.IsFenix) data.radarSys = data.radarSysFenix;
            else if (simConnect.IsIniBuilds) data.radarSys = data.radarSysIni;
            hub.Clients.All.SetFromSim(GetId(), data.radarSys);

            if (simConnect.IsIni321)
            {
                var iniValue = data.radarSysIni == 1 ? 1 : 0;
                sender.Execute(simConnect, $"{iniValue} (L:INI_WX_PWS_SWITCH) != if{{ {iniValue} (>L:INI_WX_PWS_SWITCH) }}");
                return;
            }
        }

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var code = Int16.Parse(posString!);
            simConnect.SendDataOnSimObject(new RadarSysData() { radarSys = code, radarSysFenix = code, radarSysIni = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PredictiveWindshearSysData
    {
        [SimVar("L:A32NX_SWITCH_RADAR_PWS_Position", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 pwsSwitch;
        [SimVar("L:S_WR_PRED_WS", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 pwsSwitchFenix;
        [SimVar("L:INI_WX_PWS_SWITCH", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 pwsSwitchIni;
    };

    [Component, RequiredArgsConstructor]
    public partial class PredictiveWindshearSys : DataListener<PredictiveWindshearSysData>, IRequestDataOnOpen, ISettable<bool?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly JetBridgeSender sender;

        public string GetId() => "predictiveWindshear";

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, PredictiveWindshearSysData data)
        {
            if (simConnect.IsFenix) data.pwsSwitch = data.pwsSwitchFenix;
            else if (simConnect.IsIniBuilds) data.pwsSwitch = 1 - data.pwsSwitchIni;
            hub.Clients.All.SetFromSim(GetId(), data.pwsSwitch == 1);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? isAuto)
        {
            Int32 value = isAuto == true ? 1 : 0;
            Int32 iniValue = 1 - value;
            if (simConnect.IsIni321)
            {
                sender.Execute(simConnect, $"{iniValue} (L:INI_WX_SYS_SWITCH) 1 == != if{{ {iniValue} (>L:INI_WX_SYS_SWITCH) }}");
                return;
            }
            simConnect.SendDataOnSimObject(new PredictiveWindshearSysData() {
                pwsSwitch = value, pwsSwitchFenix = value, pwsSwitchIni = iniValue });
        }
    }
}
