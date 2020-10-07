using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper
{

    public enum DATA
    {
        // Stuff intended for struct-based multiple value requests:
        [DataAttribute(typeof(ApData))] FCU_DATA = 69,
        [DataAttribute(typeof(ApHdgSelData))] AP_HDG_SEL,
        [DataAttribute(typeof(ApModeData))] AP_DATA,
        [DataAttribute(typeof(SpoilerData))] SPOILER_DATA,
        [DataAttribute(typeof(SpoilerHandle))] SPOILER_HANDLE,
    }

    enum REQUEST
    {
        [RequestAttribute(DATA.FCU_DATA, SIMCONNECT_PERIOD.SIM_FRAME)] FCU_DATA = 71,
        [RequestAttribute(DATA.AP_HDG_SEL, SIMCONNECT_PERIOD.ONCE)] FCU_HDG_SEL,
        [RequestAttribute(DATA.AP_DATA, SIMCONNECT_PERIOD.SIM_FRAME)] AP_DATA,
        [RequestAttribute(DATA.SPOILER_DATA, SIMCONNECT_PERIOD.ONCE)] MORE_SPOILER,
        [RequestAttribute(DATA.SPOILER_DATA, SIMCONNECT_PERIOD.ONCE)] LESS_SPOILER,
    }

    public enum GROUP : uint
    {
        SPOILERS = 13,
    }

    /*TODO: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.categoryattribute?view=netcore-3.1
      Way to identify specific things?
      Would we be better to have a whole class for events and their recievers which includes an ID generator? */
    enum EVENT
    {
        // Spoilers
        // The first two are sent because I couldn't work out how to send the same ones I receive without looping.
        [EventAttribute("SPOILERS_ARM_OFF")]
        DISARM_SPOILER = 42,
        [EventAttribute("SPOILERS_ARM_ON")]
        ARM_SPOILER,
        // These two are what I receive.
        [EventAttribute("SPOILERS_TOGGLE")]
        [EventGroupAttribute(GROUP.SPOILERS, true)]
        MORE_SPOILER,
        [EventAttribute("SPOILERS_ARM_TOGGLE")]
        [EventGroupAttribute(GROUP.SPOILERS, true)]
        LESS_SPOILER,
        // Autopilot stuff
        [EventAttribute("SPEED_SLOT_INDEX_SET")]
        AP_SPEED_SLOT_SET,
        [EventAttribute("AP_SPD_VAR_INC")]
        AP_SPD_UP,
        [EventAttribute("AP_SPD_VAR_DEC")]
        AP_SPD_DOWN,
        [EventAttribute("HEADING_SLOT_INDEX_SET")]
        AP_HEADING_SLOT_SET,
        [EventAttribute("HEADING_BUG_INC")]
        AP_HDG_RIGHT,
        [EventAttribute("HEADING_BUG_DEC")]
        AP_HDG_LEFT,
        [EventAttribute("HEADING_BUG_SET")]
        AP_HEADING_BUG_SET,
        [EventAttribute("ALTITUDE_SLOT_INDEX_SET")]
        AP_ALTITUDE_SLOT_SET,
        [EventAttribute("AP_ALT_VAR_INC")]
        AP_ALT_UP,
        [EventAttribute("AP_ALT_VAR_DEC")]
        AP_ALT_DOWN,
        [EventAttribute("AP_LOC_HOLD")]
        AP_TOGGLE_LOC,
        [EventAttribute("AP_APR_HOLD")]
//TODO: APPR on then LOC on leaves GS stuck on; we should request modes and react
        AP_TOGGLE_APPR,
        [EventAttribute("AUTO_THROTTLE_ARM")]
        AP_AUTOTHRUST_ARM,
        [EventAttribute("TOGGLE_FLIGHT_DIRECTOR")]
        AP_TOGGLE_FD,
        [EventAttribute("AP_VS_VAR_INC")]
        FCU_VS_UP,
        [EventAttribute("AP_VS_VAR_DEC")]
        FCU_VS_DOWN,
        [EventAttribute("AP_VS_VAR_SET_ENGLISH")]
        FCU_VS_SET,
        [EventAttribute("VS_SLOT_INDEX_SET")]
        FCU_VS_SLOT_SET,
        [EventAttribute("AP_PANEL_VS_ON")]
        AP_PANEL_VS_ON,
    }

}
