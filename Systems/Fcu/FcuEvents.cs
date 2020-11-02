namespace FSInputMapper/*.Systems.Fcu*/
{

    //TODO: convert to IEvent singletons
    public enum EVENT
    {
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
    }

}
