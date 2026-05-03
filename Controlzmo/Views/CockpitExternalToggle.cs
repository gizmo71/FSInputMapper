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
            if (true && state.Current == CameraState.CHASE) {
                simConnect.EnumerateCameraDefinitions();
            }
        }

        private void ReceiveCameraStatus(SimConnect sender, SIMCONNECT_RECV_CAMERA_STATUS data)
        {
            //... buuuuutttt... which camera?!
            // Do we have to (attempt to) acquire it first?
            Console.Error.WriteLine($"Camera API Status: acqSt {data.acquiredState} gameCont? {data.bGameControlled} dwId {data.dwID}");
        }

/*What do we do with camera defintions? 1 of 2, size 40988 with 159
        [0/0] Pilot
        [0/1] ClosePilot
        [0/2] LandingPilot
        [0/3] CoPilot
        [0/4] MCDU (Captain)
        [0/5] MCDU (FO)
        [0/6] EFB (Captain)
        [0/7] EFB (FO)
        [0/8] Overhead
        [0/9] Pedestal
        [0/10] FCU
        [0/11] Main Panel (Left)
        [0/12] Main Panel (Center)
        [0/13] Main Panel (Right)
        [0/14] Upper Overhead
        [0/15] Circuit Breaker Wall
        [0/16] Cabin
        [0/17] Cabin Window (Left Forward)
        [0/18] Cabin Window (Left Middle)
        [0/19] Cabin Window (Left Aft)
        [0/20] Cabin Window (Right Forward)
        [0/21] Cabin Window (Right Middle)
        [0/22] Cabin Window (Right Aft)
        [0/23] DEFAULT_CHASE
        [0/24] PilotVR
        [0/25] QuickView_Up
        [0/26] QuickView_Back
        [0/27] QuickView_L1
        [0/28] QuickView_L2
        [0/29] QuickView_L3
        [0/30] QuickView_R1
        [0/31] QuickView_R2
        [0/32] QuickView_R3
        [0/33] Tail
        [0/34] Belly
        [0/35] Gear
        [0/36] Forward Fuselage (Left)
        [0/37] Forward Fuselage (Right)
        [0/38] Cinematic (Front Left)
        [0/39] Cinematic (Front Right)
        [0/40] Cinematic (Rear Left)
        [0/41] Cinematic (Rear Right)
        [0/42] Cinematic (Nose)
        [0/43] ShadowCenter
        [0/44] Checklist - Fwd Left Fuselage
        [0/45] Checklist - Nose Section
        [0/46] Checklist - Nose LG
        [0/47] Checklist - Fwd Right Fuselage
        [0/48] Checklist - Underside Center Fuselage
        [0/49] Checklist - Yellow Hyd Bay and Fuel Panel
        [0/50] Checklist - Right Landing Light
        [0/51] Checklist - Right Slat 1
        [0/52] Checklist - Right Engine Oil Fill Access
        [0/53] Checklist - Right Engine Reversers
        [0/54] Checklist - Right Engine Cowl Doors
        [0/55] Checklist - Right Drain Mast
        [0/56] Checklist - Right Exhaust
        [0/57] Checklist - Right Slats
        [0/58] Checklist - Right Fuel Drain
        [0/59] Checklist - Right Refuel Coupling
        [0/60] Checklist - Right Surge Tank Inlet
        [0/61] Checklist - Right Fuel Vent Overpressure
        [0/62] Checklist - Right Wingtip
        [0/63] Checklist - Right Wingtip and Aileron
        [0/64] Checklist - Right Flaps
        [0/65] Checklist - Top Antennas
        [0/66] Checklist - Right Main Gear
        [0/67] Checklist - Right Rear Fuselage
        [0/68] Checklist - Flight Recorder
        [0/69] Checklist - Tail
        [0/70] Checklist - Tail Lower
        [0/71] Checklist - APU Access Doors
        [0/72] Checklist - APU Inlet
        [0/73] Checklist - APU Fuel Drain
        [0/74] Checklist - APU Oil Cooler Outlet
        [0/75] Checklist - APU Exhaust
        [0/76] Checklist - APU Fire Extinguisher
        [0/77] Checklist - Left Aft Passenger Door
        [0/78] Checklist - Blue Hydraulic Bay
        [0/79] Checklist - Green Hydraulic Bay
        [0/80] Checklist - Left Main Gear
        [0/81] Checklist - Left Exhaust
        [0/82] Checklist - Left Flaps
        [0/83] Checklist - Left Wingtip and Aileron
        [0/84] Checklist - Left Wingtip
        [0/85] Checklist - Left Fuel Vent Overpressure
        [0/86] Checklist - Left Surge Tank Inlet
        [0/87] Checklist - Left Refuel Coupling
        [0/88] Checklist - Left Fuel Drain
        [0/89] Checklist - Left Slats
        [0/90] Checklist - Left Drain Mast
        [0/91] Checklist - Left Engine Cowl Doors
        [0/92] Checklist - Left Engine Reversers
        [0/93] Checklist - Left Engine Oil Fill Access
        [0/94] Checklist - Left Slat 1
        [0/95] Checklist - Left Landing Light
        [0/96] Checklist - Hydraulic Reservoir Door
        [0/97] Checklist - RAT Doors
        [0/98] Checklist - VC - Cockpit Door
        [0/99] Pilot
        [0/100] ClosePilot
        [0/101] LandingPilot
        [0/102] CoPilot
        [0/103] MCDU (Captain)
        [0/104] MCDU (FO)
        [0/105] EFB (Captain)
        [0/106] EFB (FO)
        [0/107] Overhead
        [0/108] Pedestal
        [0/109] FCU
        [0/110] Main Panel (Left)
        [0/111] Main Panel (Center)
        [0/112] Main Panel (Right)
        [0/113] Upper Overhead
        [0/114] Circuit Breaker Wall
        [0/115] Cabin
        [0/116] Cabin Window (Left Forward)
        [0/117] Cabin Window (Left Middle)
        [0/118] Cabin Window (Left Aft)
        [0/119] Cabin Window (Right Forward)
        [0/120] Cabin Window (Right Middle)
        [0/121] Cabin Window (Right Aft)
        [0/122] DEFAULT_CHASE
        [0/123] PilotVR
        [0/124] QuickView_Up
        [0/125] QuickView_Back
        [0/126] QuickView_L1
        [0/127] QuickView_L2
        [0/128] QuickView_L3
        [0/129] QuickView_R1
        [0/130] QuickView_R2
        [0/131] QuickView_R3
        [0/132] Tail
        [0/133] Belly
        [0/134] Gear
        [0/135] Forward Fuselage (Left)
        [0/136] Forward Fuselage (Right)
        [0/137] Cinematic (Front Left)
        [0/138] Cinematic (Front Right)
        [0/139] Cinematic (Rear Left)
        [0/140] Cinematic (Rear Right)
        [0/141] Cinematic (Nose)
        [0/142] ShadowCenter
        [0/143] Checklist - Fwd Left Fuselage
        [0/144] Checklist - Nose Section
        [0/145] Checklist - Nose LG
        [0/146] Checklist - Fwd Right Fuselage
        [0/147] Checklist - Underside Center Fuselage
        [0/148] Checklist - Yellow Hyd Bay and Fuel Panel
        [0/149] Checklist - Right Landing Light
        [0/150] Checklist - Right Slat 1
        [0/151] Checklist - Right Engine Oil Fill Access
        [0/152] Checklist - Right Engine Reversers
        [0/153] Checklist - Right Engine Cowl Doors
        [0/154] Checklist - Right Drain Mast
        [0/155] Checklist - Right Exhaust
        [0/156] Checklist - Right Slats
        [0/157] Checklist - Right Fuel Drain
        [0/158] Checklist - Right Refuel Coupling
What do we do with camera defintions? 2 of 2, size 10268 with 39
        [1/0] Checklist - Right Surge Tank Inlet
        [1/1] Checklist - Right Fuel Vent Overpressure
        [1/2] Checklist - Right Wingtip
        [1/3] Checklist - Right Wingtip and Aileron
        [1/4] Checklist - Right Flaps
        [1/5] Checklist - Top Antennas
        [1/6] Checklist - Right Main Gear
        [1/7] Checklist - Right Rear Fuselage
        [1/8] Checklist - Flight Recorder
        [1/9] Checklist - Tail
        [1/10] Checklist - Tail Lower
        [1/11] Checklist - APU Access Doors
        [1/12] Checklist - APU Inlet
        [1/13] Checklist - APU Fuel Drain
        [1/14] Checklist - APU Oil Cooler Outlet
        [1/15] Checklist - APU Exhaust
        [1/16] Checklist - APU Fire Extinguisher
        [1/17] Checklist - Left Aft Passenger Door
        [1/18] Checklist - Blue Hydraulic Bay
        [1/19] Checklist - Green Hydraulic Bay
        [1/20] Checklist - Left Main Gear
        [1/21] Checklist - Left Exhaust
        [1/22] Checklist - Left Flaps
        [1/23] Checklist - Left Wingtip and Aileron
        [1/24] Checklist - Left Wingtip
        [1/25] Checklist - Left Fuel Vent Overpressure
        [1/26] Checklist - Left Surge Tank Inlet
        [1/27] Checklist - Left Refuel Coupling
        [1/28] Checklist - Left Fuel Drain
        [1/29] Checklist - Left Slats
        [1/30] Checklist - Left Drain Mast
        [1/31] Checklist - Left Engine Cowl Doors
        [1/32] Checklist - Left Engine Reversers
        [1/33] Checklist - Left Engine Oil Fill Access
        [1/34] Checklist - Left Slat 1
        [1/35] Checklist - Left Landing Light
        [1/36] Checklist - Hydraulic Reservoir Door
        [1/37] Checklist - RAT Doors
        [1/38] Checklist - VC - Cockpit Door*/
        private void ReceiveCameraDefinitions(SimConnect sc, SIMCONNECT_RECV_CAMERA_DEFINITION_LIST data)
        {
            Console.Error.WriteLine($"What do we do with camera defintions? {data.dwEntryNumber + 1} of {data.dwOutOf}, size {data.dwSize} with {data.dwArraySize}");
            for (int i = 0; i < data.dwArraySize; ++i) {
                Console.Error.WriteLine($"\t[{data.dwEntryNumber}/{i}] {((SIMCONNECT_CAMERA_DEFINITION_ITEM)data.rgData[i]).Str}");
            }
        }
    }
}
