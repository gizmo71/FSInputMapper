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

    /*TODO: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.categoryattribute?view=netcore-3.1
      Way to identify specific things?
      Would we be better to have a whole class for events and their recievers which includes an ID generator? */
    enum EVENT
    {
        NONE = 42,
        DISARM_SPOILER, ARM_SPOILER, MORE_SPOILER, LESS_SPOILER,
        AP_SPEED_SLOT_SET, AP_SPD_UP, AP_SPD_DOWN,
        AP_HEADING_SLOT_SET, AP_HDG_RIGHT, AP_HDG_LEFT, AP_HEADING_BUG_SET,
        AP_ALTITUDE_SLOT_SET, AP_ALT_UP, AP_ALT_DOWN,
        AP_TOGGLE_LOC, AP_TOGGLE_APPR, AP_AUTOTHRUST_ARM, AP_TOGGLE_FD,
        FCU_VS_UP, FCU_VS_DOWN, FCU_VS_SET, FCU_VS_SLOT_SET, AP_PANEL_VS_ON
    }
    enum GROUP
    {
        SPOILERS = 13,
        PRIORITY_STANDARD = 1900000000
    }

}
