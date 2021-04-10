using System;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using SimConnectzmo;

namespace Controlzmo.Systems.Radar
{
    [Component]
    public class RadarSys : ISettable<string?>
    {
        public const string id = "radarSys";

        private readonly JetBridgeSender jetbridge;

        public RadarSys(JetBridgeSender jetbridge)
        {
            this.jetbridge = jetbridge;
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:XMLVAR_A320_WeatherRadar_Sys)");
        }
    }

    [Component]
    public class PredictiveWindshearSys : ISettable<string?>
    {
        public const string id = "predictiveWindshear";

        private readonly JetBridgeSender jetbridge;

        public PredictiveWindshearSys(JetBridgeSender jetbridge)
        {
            this.jetbridge = jetbridge;
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? posString)
        {
            var pos = Int16.Parse(posString!);
            jetbridge.Execute(simConnect, $"{pos} (>L:A32NX_SWITCH_RADAR_PWS_Position)");
        }
    }
}
