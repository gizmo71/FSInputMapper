Gizmo's Controlzmo
==================

Designed mostly to enhance interaction with the default A320neo in MSFS (aka FSFS2020).

When running, it will attempt to attached SimConnect to MSFS every couple of seconds until it succeeds, and should cope gracefully if it exits.

Radio
-----

TODO

Lights
------

TODO

TODO
----

Would have to have a WASM module to send client events we could RX.
* [simvars.md](https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-simvars.md)
* [a320-events.md](https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-events.md)

Other useful things?
* `L:A32NX_AUTOPILOT_1_ACTIVE` (0/1)
* `L:XMLVAR_COM_1_VHF_C_Switch_Down` - 0 is "off", 1 is "on" (audible, lit)
* `L:XMLVAR_COM1_Volume_VHF_C` (starts at 0 but suspect that it does now't and COM2 is actually 'owning' the in-game volume)
* `L:XMLVAR_COM_Transmit_Channel` (1, 2 or 3)

Things from takeoff/after landing:
* Radar (`L:XMLVAR_A320_WeatherRadar_Sys`) & PWS (`L:A32NX_SWITCH_RADAR_PWS_Position`)
* xpndr mode (settable `TRANSPONDER STATE:1` 0-4? get/set may differ!)
* xpndr code (`TRANSPONDER CODE:1`, `K:XPNDR_SET`+BCD16)
* alt mode (`L:XMLVAR_ALT_MODE_REQUESTED`?)
* TCAS (`L:A32NX_SWITCH_TCAS_Position` and `L:A32NX_SWITCH_TCAS_Traffic_Position`)
* APU master/start (& bleed?) - `L:A32NX_APU_BLEED_AIR_VALVE_OPEN`,  lots of things with `L:*_OVHD_APU_*`
