using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;

namespace Controlzmo.Views
{
    [Component]
    [RequiredArgsConstructor]
    public partial class CockpitExternalToggleStick : IButtonCallback<UrsaMinorFighterR>
    {
        private readonly CockpitExternalToggleHotas hotas;

        public int GetButton() => UrsaMinorFighterR.BUTTON_SQUARE_HAT_PRESS;

        public void OnPress(ExtendedSimConnect simConnect) => hotas.OnPress(simConnect);
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class CockpitExternalToggleHotas : IButtonCallback<T16000mHotas>
    {
        private readonly ILogger<CockpitExternalToggleHotas> _logger;
        private readonly CameraState state;

        public int GetButton() => T16000mHotas.BUTTON_SIDE_RED;
        public void OnPress(ExtendedSimConnect simConnect)
        {
            if (state.Current == CameraState.CHASE) {
                _logger.LogWarning($"Requesting cockpit for {state.Current}");
                state.Current = CameraState.COCKPIT;
            }  else if (state.Current == CameraState.COCKPIT || state.Current == CameraState.WORLD_MAP) {
                _logger.LogWarning($"Requesting chase for {state.Current}");
                state.Current = CameraState.CHASE;
            } else
                _logger.LogWarning($"Wrong camera state for chase view toggle {state.Current}");
        }

//https://docs.flightsimulator.com/msfs2024/flighting/html/6_Programming_APIs/SimConnect/API_Reference/Camera/Camera_API.htm
//https://devsupport.flightsimulator.com/t/camera-api-discussion/17193
        private bool isCallbackDefined;
        public void OnRelease(ExtendedSimConnect simConnect)
        {
            if (!isCallbackDefined)
            {
                simConnect.OnRecvCameraDefinitionList += ReceiveCameraDefinitions;
                simConnect.OnRecvCameraStatus += ReceiveCameraStatus;
                isCallbackDefined = true;
            }
            if (false && state.Current == CameraState.CHASE) {
                simConnect.EnumerateCameraDefinitions();
            }
        }

        private void ReceiveCameraStatus(SimConnect sender, SIMCONNECT_RECV_CAMERA_STATUS data)
        {
            //... buuuuutttt... which camera?!
            // Do we have to (attempt to) acquire it first?
            Console.Error.WriteLine($"Camera API Status: acqSt {data.acquiredState} gameCont? {data.bGameControlled} dwId {data.dwID}");
        }

        /*What do we do with camera defintions? 1 of 2, size40988 with 159
       [0] Pilot
       [1] ClosePilot
       [2] LandingPilot
       [3] CoPilot
       [4] MCDU (Captain)
       [5] MCDU (FO)
       [6] EFB (Captain)
       [7] EFB (FO)
       [8] Overhead
       [9] Pedestal
       [10] FCU
       [11] Main Panel (Left)
       [12] Main Panel (Center)
       [13] Main Panel (Right)
       [14] Upper Overhead
       [15] Circuit Breaker Wall
       [16] Cabin
       [17] Cabin Window (Left Forward)
       [18] Cabin Window (Left Middle)
       [19] Cabin Window (Left Aft)
       [20] Cabin Window (Right Forward)
       [21] Cabin Window (Right Middle)
       [22] Cabin Window (Right Aft)
       [23] DEFAULT_CHASE
       [24] PilotVR
       [25] QuickView_Up
       [26] QuickView_Back
       [27] QuickView_L1
       [28] QuickView_L2
       [29] QuickView_L3
       [30] QuickView_R1
       [31] QuickView_R2
       [32] QuickView_R3
       [33] Tail
       [34] Belly
       [35] Gear
       [36] Forward Fuselage (Left)
       [37] Forward Fuselage (Right)
       [38] Cinematic (Front Left)
       [39] Cinematic (Front Right)
       [40] Cinematic (Rear Left)
       [41] Cinematic (Rear Right)
       [42] Cinematic (Nose)
       [43] ShadowCenter
       [44] Checklist - Fwd Left Fuselage
       [45] Checklist - Nose Section
       [46] Checklist - Nose LG
       [47] Checklist - Fwd Right Fuselage
       [48] Checklist - Underside Center Fuselage
       [49] Checklist - Yellow Hyd Bay and Fuel Panel
       [50] Checklist - Right Landing Light
       [51] Checklist - Right Slat 1
       [52] Checklist - Right Engine Oil Fill Access
       [53] Checklist - Right Engine Reversers
       [54] Checklist - Right Engine Cowl Doors
       [55] Checklist - Right Drain Mast
       [56] Checklist - Right Exhaust
       [57] Checklist - Right Slats
       [58] Checklist - Right Fuel Drain
       [59] Checklist - Right Refuel Coupling
       [60] Checklist - Right Surge Tank Inlet
       [61] Checklist - Right Fuel Vent Overpressure
       [62] Checklist - Right Wingtip
       [63] Checklist - Right Wingtip and Aileron
       [64] Checklist - Right Flaps
       [65] Checklist - Top Antennas
       [66] Checklist - Right Main Gear
       [67] Checklist - Right Rear Fuselage
       [68] Checklist - Flight Recorder
       [69] Checklist - Tail
       [70] Checklist - Tail Lower
       [71] Checklist - APU Access Doors
       [72] Checklist - APU Inlet
       [73] Checklist - APU Fuel Drain
       [74] Checklist - APU Oil Cooler Outlet
       [75] Checklist - APU Exhaust
       [76] Checklist - APU Fire Extinguisher
       [77] Checklist - Left Aft Passenger Door
       [78] Checklist - Blue Hydraulic Bay
       [79] Checklist - Green Hydraulic Bay
       [80] Checklist - Left Main Gear
       [81] Checklist - Left Exhaust
       [82] Checklist - Left Flaps
       [83] Checklist - Left Wingtip and Aileron
       [84] Checklist - Left Wingtip
       [85] Checklist - Left Fuel Vent Overpressure
       [86] Checklist - Left Surge Tank Inlet
       [87] Checklist - Left Refuel Coupling
       [88] Checklist - Left Fuel Drain
       [89] Checklist - Left Slats
       [90] Checklist - Left Drain Mast
       [91] Checklist - Left Engine Cowl Doors
       [92] Checklist - Left Engine Reversers
       [93] Checklist - Left Engine Oil Fill Access
       [94] Checklist - Left Slat 1
       [95] Checklist - Left Landing Light
       [96] Checklist - Hydraulic Reservoir Door
       [97] Checklist - RAT Doors
       [98] Checklist - VC - Cockpit Door
       [99] Pilot
       [100] ClosePilot
       [101] LandingPilot
       [102] CoPilot
       [103] MCDU (Captain)
       [104] MCDU (FO)
       [105] EFB (Captain)
       [106] EFB (FO)
       [107] Overhead
       [108] Pedestal
       [109] FCU
       [110] Main Panel (Left)
       [111] Main Panel (Center)
       [112] Main Panel (Right)
       [113] Upper Overhead
       [114] Circuit Breaker Wall
       [115] Cabin
       [116] Cabin Window (Left Forward)
       [117] Cabin Window (Left Middle)
       [118] Cabin Window (Left Aft)
       [119] Cabin Window (Right Forward)
       [120] Cabin Window (Right Middle)
       [121] Cabin Window (Right Aft)
       [122] DEFAULT_CHASE
       [123] PilotVR
       [124] QuickView_Up
       [125] QuickView_Back
       [126] QuickView_L1
       [127] QuickView_L2
       [128] QuickView_L3
       [129] QuickView_R1
       [130] QuickView_R2
       [131] QuickView_R3
       [132] Tail
       [133] Belly
       [134] Gear
       [135] Forward Fuselage (Left)
       [136] Forward Fuselage (Right)
       [137] Cinematic (Front Left)
       [138] Cinematic (Front Right)
       [139] Cinematic (Rear Left)
       [140] Cinematic (Rear Right)
       [141] Cinematic (Nose)
       [142] ShadowCenter
       [143] Checklist - Fwd Left Fuselage
       [144] Checklist - Nose Section
       [145] Checklist - Nose LG
       [146] Checklist - Fwd Right Fuselage
       [147] Checklist - Underside Center Fuselage
       [148] Checklist - Yellow Hyd Bay and Fuel Panel
       [149] Checklist - Right Landing Light
       [150] Checklist - Right Slat 1
       [151] Checklist - Right Engine Oil Fill Access
       [152] Checklist - Right Engine Reversers
       [153] Checklist - Right Engine Cowl Doors
       [154] Checklist - Right Drain Mast
       [155] Checklist - Right Exhaust
       [156] Checklist - Right Slats
       [157] Checklist - Right Fuel Drain
       [158] Checklist - Right Refuel Coupling
       [0] Checklist - Right Surge Tank Inlet
       [1] Checklist - Right Fuel Vent Overpressure
       [2] Checklist - Right Wingtip
       [3] Checklist - Right Wingtip and Aileron
       [4] Checklist - Right Flaps
       [5] Checklist - Top Antennas
       [6] Checklist - Right Main Gear
       [7] Checklist - Right Rear Fuselage
       [8] Checklist - Flight Recorder
       [9] Checklist - Tail
       [10] Checklist - Tail Lower
       [11] Checklist - APU Access Doors
       [12] Checklist - APU Inlet
       [13] Checklist - APU Fuel Drain
       [14] Checklist - APU Oil Cooler Outlet
       [15] Checklist - APU Exhaust
       [16] Checklist - APU Fire Extinguisher
       [17] Checklist - Left Aft Passenger Door
       [18] Checklist - Blue Hydraulic Bay
       [19] Checklist - Green Hydraulic Bay
       [20] Checklist - Left Main Gear
       [21] Checklist - Left Exhaust
       [22] Checklist - Left Flaps
       [23] Checklist - Left Wingtip and Aileron
       [24] Checklist - Left Wingtip
       [25] Checklist - Left Fuel Vent Overpressure
       [26] Checklist - Left Surge Tank Inlet
       [27] Checklist - Left Refuel Coupling
       [28] Checklist - Left Fuel Drain
       [29] Checklist - Left Slats
       [30] Checklist - Left Drain Mast
       [31] Checklist - Left Engine Cowl Doors
       [32] Checklist - Left Engine Reversers
       [33] Checklist - Left Engine Oil Fill Access
       [34] Checklist - Left Slat 1
       [35] Checklist - Left Landing Light
       [36] Checklist - Hydraulic Reservoir Door
       [37] Checklist - RAT Doors
       [38] Checklist - VC - Cockpit Door*/
        private void ReceiveCameraDefinitions(SimConnect sc, SIMCONNECT_RECV_CAMERA_DEFINITION_LIST data)
        {
            Console.Error.WriteLine($"What do we do with camera defintions? {data.dwEntryNumber + 1} of {data.dwOutOf}, size{data.dwSize} with {data.dwArraySize}");
            for (int i = 0; i < data.dwArraySize; ++i) {
                Console.Error.WriteLine($"\t[{i}] {((SIMCONNECT_CAMERA_DEFINITION_ITEM)data.rgData[i]).Str}");
            }
        }
    }
}
