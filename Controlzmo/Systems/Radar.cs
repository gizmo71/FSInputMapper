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
        protected override double Default() => -1.0;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        public string GetId() => "radarSys";

        protected override double? Value { set => hub.Clients.All.SetFromSim(GetId(), base.Value = value); }

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:XMLVAR_A320_WeatherRadar_Sys)");
        }
    }

    [Component]
    public class PredictiveWindshearSys : ISettable<bool?>, IOnSimConnection
    {
        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly LVarRequester lvarRequester;

        public PredictiveWindshearSys(IServiceProvider sp)
        {
            jetbridge = sp.GetRequiredService<JetBridgeSender>();
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            (lvarRequester = sp.GetRequiredService<LVarRequester>()).LVarUpdated += UpdateLVar;
        }

        public string GetId() => "predictiveWindshear";

        public void OnConnection(ExtendedSimConnect simConnect)
        {
            lvarRequester.Request(simConnect, "A32NX_SWITCH_RADAR_PWS_Position", 4000, -1.0);
        }

        private void UpdateLVar(string name, double? newValue)
        {
            if (name == "A32NX_SWITCH_RADAR_PWS_Position")
                hub.Clients.All.SetFromSim(GetId(), newValue);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? isAuto)
        {
            var value = isAuto == true ? 1 : 0;
            jetbridge.Execute(simConnect, $"{value} (>L:A32NX_SWITCH_RADAR_PWS_Position)");
        }
    }
}
