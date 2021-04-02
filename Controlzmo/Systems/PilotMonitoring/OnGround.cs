namespace Controlzmo.Systems.PilotMonitoring
{
    //TODO: just monitor on ground status and start/stop other PM listeners based on that
/*On landing, would be good to detect reversers (how? A32NX_AUTOTHRUST_REVERSE:1/2?),
spoilers popping (see A320_Neo_LowerECAM_FTCL.js, just using SPOILERS LEFT/RIGHT POSITION and ON GROUND?)*/
/* Other useful things?
 * A32NX_AUTOPILOT_1_ACTIVE (0/1)
 * Lots of things with *_OVHD_APU_*
 * XMLVAR_COM_1_VHF_C_Switch_Down - 0 is "off", 1 is "on" (audible, lit)
 * XMLVAR_COM1_Volume_VHF_C (starts at 0 but suspect that it does nowt and COM2 is actually 'owning' the in-game volume)
 * XMLVAR_COM_Transmit_Channel (1, 2 or 3)
 */
    public class OnGround
    {
    }
}
