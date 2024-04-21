using Controlzmo.Systems.EfisControlPanel;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EngineWarmupData
    {
        [SimVar("ENG COMBUSTION:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 combustion1;
        [SimVar("ENG COMBUSTION:2", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 combustion2;
        [SimVar("ABSOLUTE TIME", "seconds", SIMCONNECT_DATATYPE.FLOAT64, 2.5f)]
        public Double now; // Only has second granularity, despite apparent precision; stops for pauses
    };

    [Component]
    public class EngineWarmupListener : DataListener<EngineWarmupData>
    {
        private readonly JetBridgeSender jetbridge;
        private readonly ChronoButton chronoButton;

        private bool isArmed = false;
        private Double? warmAt = null;

        public EngineWarmupListener(IServiceProvider serviceProvider)
        {
            jetbridge = serviceProvider.GetRequiredService<JetBridgeSender>();
            chronoButton = serviceProvider.GetRequiredService<ChronoButton>();
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }

        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            var period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
            if (!isOnGround) {
                warmAt = null;
                isArmed = false;
            }
        }

        public override void Process(ExtendedSimConnect simConnect, EngineWarmupData data)
        {
            //TODO: support different numbers of engines... MSFS now supports up to 16...
            int engines = 2;
            int running = data.combustion1 + data.combustion2;
            if (running == engines - 1 && !isArmed)
            {
                warmAt = null;
                isArmed = true;
            }
            else if (running == engines && isArmed)
            {
//TODO: allow the warmup time to be configured, e.g. for Frontier (5m).
                warmAt = data.now + 3 * 60.0;
                chronoButton.SetInSim(simConnect, null);
                isArmed = false;
            }
            else if (running < engines - 1)
            {
                isArmed = false;
                warmAt = null;
            }
            else if (engines == running && warmAt != null && data.now >= warmAt) // We are not armed at this point.
            {
                //TODO: is there a Fenix equivalent? Could we just hit Chrono twice?
                jetbridge.Execute(simConnect, "1 (>L:A32NX_CABIN_READY)");
                isArmed = false;
                warmAt = null;
            }
        }
    }
}
