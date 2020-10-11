using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    public enum STRUCT
    {
    }

    enum REQUEST
    {
        [Request(typeof(ApData), SIMCONNECT_PERIOD.SIM_FRAME)] FCU_DATA = 71,
        [Request(typeof(ApHdgSelData), SIMCONNECT_PERIOD.ONCE)] FCU_HDG_SEL,
        [Request(typeof(ApModeData), SIMCONNECT_PERIOD.SIM_FRAME)] AP_DATA,
        [Request(typeof(SpoilerData), SIMCONNECT_PERIOD.ONCE)] MORE_SPOILER,
        [Request(typeof(SpoilerHandle), SIMCONNECT_PERIOD.ONCE)] LESS_SPOILER,
    }

    public enum GROUP : uint
    {
        [HighestMaskablePriorityGroup]
        SPOILERS = 13,
    }

    /*TODO: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.categoryattribute?view=netcore-3.1
      Way to identify specific things?
      Would we be better to have a whole class for events and their recievers which includes an ID generator? */
    enum EVENT
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
    }

}
