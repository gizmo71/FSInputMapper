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
    public class RadarSys : ISettable<string?>, IOnSimConnection
    {
        private const string id = "radarSys";

        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly LVarRequester lvarRequester;

        public RadarSys(IServiceProvider sp)
        {
            jetbridge = sp.GetRequiredService<JetBridgeSender>();
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            (lvarRequester = sp.GetRequiredService<LVarRequester>()).LVarUpdated += UpdateLVar;
        }

        public string GetId() => id;

        public void OnConnection(ExtendedSimConnect simConnect)
        {
            lvarRequester.Request(simConnect, "XMLVAR_A320_WeatherRadar_Sys", 4000, -1.0);
        }

        private void UpdateLVar(string name, double? newValue)
        {
            if (name == "XMLVAR_A320_WeatherRadar_Sys")
                hub.Clients.All.SetFromSim(id, newValue);
        }

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:XMLVAR_A320_WeatherRadar_Sys)");
        }
    }

    [Component]
    public class PredictiveWindshearSys : ISettable<bool?>, IOnSimConnection
    {
        private const string id = "predictiveWindshear";

        private readonly JetBridgeSender jetbridge;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly LVarRequester lvarRequester;

        public PredictiveWindshearSys(IServiceProvider sp)
        {
            jetbridge = sp.GetRequiredService<JetBridgeSender>();
            hub = sp.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            (lvarRequester = sp.GetRequiredService<LVarRequester>()).LVarUpdated += UpdateLVar;
        }

        public string GetId() => id;

        public void OnConnection(ExtendedSimConnect simConnect)
        {
            lvarRequester.Request(simConnect, "A32NX_SWITCH_RADAR_PWS_Position", 4000, -1.0);
        }

        private void UpdateLVar(string name, double? newValue)
        {
            if (name == "A32NX_SWITCH_RADAR_PWS_Position")
                hub.Clients.All.SetFromSim(id, newValue);
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool? isAuto)
        {
            var value = isAuto == true ? 1 : 0;
            jetbridge.Execute(simConnect, $"{value} (>L:A32NX_SWITCH_RADAR_PWS_Position)");
        }
    }
}
