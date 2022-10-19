using System;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Systems.Radar
{
    [Component]
    public class RadarSys : LVar, IOnSimConnection, ISettable<string?>
    {
        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public RadarSys(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
        }

        protected override string LVarName() => "XMLVAR_A320_WeatherRadar_Sys";
        protected override int Milliseconds() => 4000;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        public string GetId() => "radarSys";

        protected override double? Value { set { hub.Clients.All.SetFromSim(GetId(), base.Value = value); } }

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:{LVarName()})");
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
