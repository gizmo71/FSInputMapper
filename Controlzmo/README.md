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

https://docs.microsoft.com/en-us/previous-versions/microsoft-esp/cc526953(v=msdn.10)?redirectedfrom=MSDN#theinfix2postfixtool

Other useful things?
* `L:A32NX_AUTOPILOT_1_ACTIVE` (0/1) along with event `A32NX_FCU_AP_1/2_PUSH` (is it "H:"? doesn't seem to work in JetBridge)
* `L:XMLVAR_COM_1_VHF_C_Switch_Down` - 0 is "off", 1 is "on" (audible, lit)
* `L:XMLVAR_COM1_Volume_VHF_C` (starts at 0 but suspect that it does now't and COM2 is actually 'owning' the in-game volume)
* `L:XMLVAR_COM_Transmit_Channel` (1, 2 or 3)

Tiller seems to be mapped to `A:GEAR STEER ANGLE PCT` on ground, or `A:STEER INPUT CONTROL` in air.

Things from takeoff/after landing:
* Radar (`L:XMLVAR_A320_WeatherRadar_Sys` settable 0-2) & PWS (`L:A32NX_SWITCH_RADAR_PWS_Position` settable 0,1)
* xpndr mode (settable `TRANSPONDER STATE:1` 1 Stby or Auto, 3 on without Alt Rptg, 4 on with Alt Rptg)
  1 is standby or auto, 3/4 appear to be "On" in sim with alt rptg in off/on respectively, but if one, doesn't report the other when alt rptg is switched :-(
  In auto, is unsettable. It's likely that the value changes once airbourne if in standby.
  Setting to 1 from On always turns it to Standby, regardless of Alt Rptg.
  If Alt Rptg is On, only 4 will set it to On.
  If Alt Rptg is Off, only 3 will set it to On.
* xpndr code (`TRANSPONDER CODE:1` readonly, `K:XPNDR_SET`+BCD16 works)
* alt mode (`I:XMLVAR_ALT_MODE_REQUESTED`, `I:XMLVAR_Auto`, 1 if Auto, 0 if On or Stby - can't set at all)
* Ident? (local event `A320_Neo_ATC_BTN_IDENT`?)
* TCAS (`L:A32NX_SWITCH_TCAS_Position` 0-2 and `L:A32NX_SWITCH_TCAS_Traffic_Position` 0-3, both settable)

* APU master/start (& bleed?) - `L:A32NX_APU_BLEED_AIR_VALVE_OPEN`,  lots of things with `L:*_OVHD_APU_*`
* States: `L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE`, `L:A32NX_OVHD_APU_START_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`