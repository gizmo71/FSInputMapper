using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
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
        [SimVar(RadarSys.LVarName, "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 radarSys;
    };

    [Component]
    public class RadarSys : DataListener<RadarSysData>, IOnSimConnection, ISettable<string?>
    {
        internal const string LVarName = "L:XMLVAR_A320_WeatherRadar_Sys";

        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public RadarSys(IServiceProvider serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
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
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>{LVarName})");
        }
    }

    [Component]
    public class PredictiveWindshearSys : LVar, ISettable<bool?>, IOnSimConnection
    {
        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public PredictiveWindshearSys(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        protected override string LVarName() => "A32NX_SWITCH_RADAR_PWS_Position";
        protected override int Milliseconds() => 4000;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        public string GetId() => "predictiveWindshear";

        protected override double? Value { set => hub.Clients.All.SetFromSim(GetId(), base.Value = value); }

        public void SetInSim(ExtendedSimConnect simConnect, bool? isAuto)
        {
            var value = isAuto == true ? 1 : 0;
            jetbridge.Execute(simConnect, $"{value} (>L:{LVarName()})");
        }
    }
}
