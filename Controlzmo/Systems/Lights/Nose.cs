using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Lights
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TaxiLightData
    {
        [SimVar("L:S_OH_EXT_LT_NOSE", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fenix;
        [SimVar("L:INI_TAXI_LIGHT_SWITCH", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 ini;
        [SimVar("L:MSATR_ELTS_TAXI_TO", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 atr;
        [SimVar("L:LIGHTING_LANDING_1", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 fbw;
        [SimVar("LIGHT TAXI", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 standard;
        [SimVar("L:S_OH_EXT_LT_RWY_TURNOFF", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fenixTurnoff;
        [SimVar("L:INI_TURNOFF_LIGHT_SWITCH", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int iniTurnoff;
        [SimVar("LIGHT TAXI:2", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fbwTurnoffLeft;
        [SimVar("LIGHT TAXI:3", "Number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fbwTurnoffRight;
    }

    [Component, RequiredArgsConstructor]
    public partial class TaxiLightSystem : DataListener<TaxiLightData>, IRequestDataOnOpen, ISettable<bool>
    {
        private readonly JetBridgeSender sender;
        private readonly LightState state;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public string GetId() => "lightsTaxi";
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect sc, TaxiLightData data)
        {
            if (sc.IsFenix)
                data.standard = data.fenix > 0 && data.fenixTurnoff == 1 ? 1 : 0;
            else if (sc.IsIniBuilds)
                data.standard = data.ini < 2 && data.iniTurnoff == 1 ? 1 : 0;
            else if (sc.IsAtr)
                data.standard = data.atr == 1 ? 1 : 0;
            else if (sc.IsFBW)
                data.standard = data.fbw < 2 && data.fbwTurnoffLeft == 1 && data.fbwTurnoffRight == 1 ? 1 : 0;
            hub.Clients.All.SetFromSim(GetId(), state.IsTaxiOn = (data.standard == 1));
        }

        public void SetInSim(ExtendedSimConnect simConnect, bool isOn)
        {
            // Runway turnoffs, for aircraft with them
            uint turnoffCode = isOn ? 1u : 0u;
            if (simConnect.IsFBW)
                sender.Execute(simConnect, $"{turnoffCode} d 3 r (>K:2:TAXI_LIGHTS_SET) 2 r (>K:2:TAXI_LIGHTS_SET)");
            else if (simConnect.IsFenix)
                sender.Execute(simConnect, $"{turnoffCode} (>L:S_OH_EXT_LT_RWY_TURNOFF)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, $"{turnoffCode} (>L:INI_TURNOFF_LIGHT_SWITCH)");

            // Nose or taxi lights
            if (simConnect.IsA380X)
            {
                var code = isOn ? (state.IsLandingOn ? 0u : 1u) : 2u;
                sender.Execute(simConnect, $"{code} (>B:LIGHTING_LANDING_1_SET)");
            }
            else if (simConnect.IsFBW) // tested with A339 only TODO test with A20N
            {
                var noseCode = isOn ? (state.IsLandingOn ? 0u : 1u) : 2u;
                sender.Execute(simConnect, $"{noseCode} (>B:LIGHTING_LANDING_1_SET)");
            }
            else if (simConnect.IsFenix)
            {
                var code = isOn ? (state.IsLandingOn ? 2u : 1u) : 0u;
                sender.Execute(simConnect, $"{code} (>L:S_OH_EXT_LT_NOSE)");
            }
            else if (simConnect.IsIniBuilds)
            {
                var code = isOn ? (state.IsLandingOn ? 0u : 1u) : 2u;
                sender.Execute(simConnect, $"{code} (>L:INI_TAXI_LIGHT_SWITCH)");
            }
            else if (simConnect.IsAtr)
                sender.Execute(simConnect, $"{(isOn ? 1 : 0)} (>L:MSATR_ELTS_TAXI_TO)");
            state.IsTaxiOn = isOn;
        }
    }
}
