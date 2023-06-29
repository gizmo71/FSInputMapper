﻿using Controlzmo.Hubs;
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
    };

    [Component]
    public class RadarSys : DataListener<RadarSysData>, IOnSimConnection, ISettable<string?>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public RadarSys(IServiceProvider serviceProvider)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public void OnConnection(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public string GetId() => "radarSys";

        public override void Process(ExtendedSimConnect simConnect, RadarSysData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.radarSys);
        }

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            simConnect.SendDataOnSimObject(new RadarSysData() { radarSys = Int16.Parse(posString!) });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PredictiveWindshearSysData
    {
        [SimVar("L:A32NX_SWITCH_RADAR_PWS_Position", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 pwsSwitch;
    };

    [Component]
    public class PredictiveWindshearSys : DataListener<PredictiveWindshearSysData>, ISettable<bool?>, IOnSimConnection
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public PredictiveWindshearSys(IServiceProvider serviceProvider)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        public string GetId() => "predictiveWindshear";

        public void OnConnection(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);

        public override void Process(ExtendedSimConnect simConnect, PredictiveWindshearSysData data)
        {
            hub.Clients.All.SetFromSim(GetId(), data.pwsSwitch == 1);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? isAuto)
        {
            simConnect.SendDataOnSimObject(new PredictiveWindshearSysData() { pwsSwitch = isAuto == true ? 1 : 0 });
        }
    }
}
