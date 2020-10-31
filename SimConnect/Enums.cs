namespace FSInputMapper
{

    public enum GROUP : uint
    {
        [HighestMaskablePriorityGroup]
        SPOILERS = 13,
        [HighestMaskablePriorityGroup]
        ENGINE,
    }

    /*TODO: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.categoryattribute?view=netcore-3.1
      Way to identify specific things?
      Would we be better to have a whole class for events and their recievers which includes an ID generator? */
    public enum EVENT
    {
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
        [Event("NAV_LIGHTS_SET")]
        LIGHTS_NAV_SET,
        [Event("TOGGLE_RECOGNITION_LIGHTS")]
        LIGHTS_RECOGNITION_TOGGLE,
        [Event("LANDING_LIGHTS_SET")]
        LIGHTS_LANDING_SET,
        [Event("TAXI_LIGHTS_SET")]
        LIGHTS_TAXI_SET,
        // These "work" but the visible lever doesn't move; that stops reverse being set like this too. :-(
        [Event("THROTTLE1_SET")]
        ENGINE_THROTTLE_1_SET,
        [Event("THROTTLE2_SET")]
        ENGINE_THROTTLE_2_SET,
    }

}
