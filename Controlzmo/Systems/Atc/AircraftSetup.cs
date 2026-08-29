using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.Atc
{
    [Component] public class Com1VolumeSetEvent : IEvent { public string SimEvent() => "COM1_VOLUME_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class AircraftSetup : ISettable<string?>
    {
        private readonly JetBridgeSender sender;
        private readonly Com1VolumeSetEvent volume;
        private readonly OperationalFlightPlan ofp;

        public string GetId() => "initAircraftState";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            simConnect.SendEvent(volume, 100);

            if (simConnect.IsAtr || simConnect.IsHorizonB789)
            {
                for (var i = 0; i++ < 2; ) {
                    var command = $"1 (>L:XMLVAR_YOKEHIDDEN{i})";
                    if (simConnect.IsAtr)
                        command += $" 1 (>L:MSATR_MICROPHONE_{(i == 1 ? "LEFT" : "RIGHT")}_HIDDEN)";
                    sender.Execute(simConnect, command);
                }
            }

            if (simConnect.IsAtr)
            {
                var atrToggleStorm = "(>B:LIGHTING_LIGHTING_SWITCH_STORM_TOGGLE)"; // Bump this to make integrated panel lights work (known bug)
                sender.Execute(simConnect, atrToggleStorm);
                BoundedSet(simConnect, false, "L:MSATR_ILTS_DOME", 1); // 0 bright, 1 dim, 2 off
                // 5 is off, 4 is minimum, down to 0 as brightest
                BoundedSet(simConnect, false, "L:MSATR_ILTS_MIP_PED_KNOB", 4);
                BoundedSet(simConnect, false, "L:MSATR_ILTS_OVHD_KNOB", 4);
                // These are 0 off, 5 minimum, 100 brightest
                BoundedSet(simConnect, true, "L:MSATR_ILTS_READING_CPT", 5);
                BoundedSet(simConnect, true, "L:MSATR_ILTS_CONSOLE_CPT", 5);
                BoundedSet(simConnect, true, "L:MSATR_ILTS_FLOOD_KNOB", 5);
                sender.Execute(simConnect, atrToggleStorm);
            }
            else if (simConnect.IsFenix)
            {
                sender.Execute(simConnect, "0 (>L:S_EFB_VISIBLE_FO) 0 (>L:S_EFB_CHARGING_CABLE_FO) 0 (>L:S_WINDOW_BLINDS_FO) 1.0 (>L:A_MIP_LIGHTING_FLOOD_MAIN)");
                sender.Execute(simConnect, "2 (>L:S_XPDR_OPERATION)");
            }
            else if (simConnect.IsA380X)
                sender.Execute(simConnect, "2 (>L:A32NX_TRANSPONDER_MODE) 1 (>L:A380X_RMP_1_VHF_TX_1)");
            else if (simConnect.IsFBW)
                sender.Execute(simConnect, "2 (>L:A32NX_TRANSPONDER_MODE)");
            else if (simConnect.IsIniBuilds)
                sender.Execute(simConnect, "2 (>L:INI_TCAS_STBY_STATE)");

            ofp.ReadVSpeeds(simConnect);
        }

        private void BoundedSet(ExtendedSimConnect sc, bool isMinimum, string var, int value)
        {
            sender.Execute(sc, $"{value} ({var}) {(isMinimum ? "max" : "min")} (>{var})");
        }
    }
}
