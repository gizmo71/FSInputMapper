﻿using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    public enum GROUP : uint
    {
        [HighestMaskablePriorityGroup]
        SPOILERS = 13,
    }

    /*TODO: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.categoryattribute?view=netcore-3.1
      Way to identify specific things?
      Would we be better to have a whole class for events and their recievers which includes an ID generator? */
    public enum EVENT
    {
        // Spoilers
        // The first two are sent because I couldn't work out how to send the same ones I receive without looping.
        [Event("SPOILERS_ARM_OFF")]
        DISARM_SPOILER = 42,
        [Event("SPOILERS_ARM_ON")]
        ARM_SPOILER,
        // These two are what I receive.
        [Event("SPOILERS_TOGGLE")]
        [EventGroup(GROUP.SPOILERS, true)]
        MORE_SPOILER,
        [Event("SPOILERS_ARM_TOGGLE")]
        [EventGroup(GROUP.SPOILERS, true)]
        LESS_SPOILER,
        // Autopilot stuff
        [Event("SPEED_SLOT_INDEX_SET")]
        AP_SPEED_SLOT_SET,
        [Event("AP_SPD_VAR_INC")]
        AP_SPD_UP,
        [Event("AP_SPD_VAR_DEC")]
        AP_SPD_DOWN,
        [Event("HEADING_SLOT_INDEX_SET")]
        AP_HEADING_SLOT_SET,
        [Event("HEADING_BUG_INC")]
        AP_HDG_RIGHT,
        [Event("HEADING_BUG_DEC")]
        AP_HDG_LEFT,
        [Event("HEADING_BUG_SET")]
        AP_HEADING_BUG_SET,
        [Event("ALTITUDE_SLOT_INDEX_SET")]
        AP_ALTITUDE_SLOT_SET,
        [Event("AP_ALT_VAR_INC")]
        AP_ALT_UP,
        [Event("AP_ALT_VAR_DEC")]
        AP_ALT_DOWN,
        [Event("AP_LOC_HOLD")]
        AP_TOGGLE_LOC,
        [Event("AP_APR_HOLD")]
//TODO: APPR on then LOC on leaves GS stuck on; we should request modes and react
        AP_TOGGLE_APPR,
        [Event("AUTO_THROTTLE_ARM")]
        AP_AUTOTHRUST_ARM,
        [Event("TOGGLE_FLIGHT_DIRECTOR")]
        AP_TOGGLE_FD,
        [Event("AP_VS_VAR_INC")]
        FCU_VS_UP,
        [Event("AP_VS_VAR_DEC")]
        FCU_VS_DOWN,
        [Event("AP_VS_VAR_SET_ENGLISH")]
        FCU_VS_SET,
        [Event("VS_SLOT_INDEX_SET")]
        FCU_VS_SLOT_SET,
        [Event("AP_PANEL_VS_ON")]
        AP_PANEL_VS_ON,
        [Event("STROBES_SET")]
        LIGHTS_STROBES_SET,
        [Event("TOGGLE_BEACON_LIGHTS")]
        LIGHTS_BEACON_TOGGLE,
        [Event("TOGGLE_WING_LIGHTS")]
        LIGHTS_WING_TOGGLE,
        [Event("LOGO_LIGHTS_SET")]
        LIGHTS_LOGO_SET,
        [Event("TOGGLE_NAV_LIGHTS")]
        LIGHTS_NAV_TOGGLE,
        [Event("TOGGLE_RECOGNITION_LIGHTS")]
        LIGHTS_RECOGNITION_TOGGLE,
        [Event("LANDING_LIGHTS_SET")]
        LIGHTS_LANDING_SET,
        [Event("TAXI_LIGHTS_SET")]
        LIGHTS_TAXI_SET,
#if FALSE
        // Hmm - pot n is 1 to 31; 0 might be nav/logo; 1 is the nose light; combo of 2+3 might be RWY_TURNOFF
        // A320_NEO_INTERIOR.xml
        [Event("LIGHT_POTENTIOMETER_2_SET")]
        LIGHTS_LANDING_RIGHT_SET,
        [Event("LIGHT_POTENTIOMETER_3_SET")]
        LIGHTS_LANDING_LEFT_SET,
        [Event("LIGHT_POTENTIOMETER_24_SET")]
        LIGHTS_STROBE_SET,
        [Event("LIGHT_POTENTIOMETER_INC")] // Presumably one passes the potentiometer # as a parameter
        LIGHTS_POTENTIOMETER_INC,
        [Event("LIGHT_POTENTIOMETER_DEC")] // Presumably one passes the potentiometer # as a parameter
        LIGHTS_POTENTIOMETER_DEC,
#endif
    }

}
