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
* https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-simvars.md
* https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-events.md

APU controls - not part of PM - A32NX_APU_BLEED_AIR_VALVE_OPEN,  lots of things with *_OVHD_APU_*

Other useful things?
* A32NX_AUTOPILOT_1_ACTIVE (0/1)
* XMLVAR_COM_1_VHF_C_Switch_Down - 0 is "off", 1 is "on" (audible, lit)
* XMLVAR_COM1_Volume_VHF_C (starts at 0 but suspect that it does nowt and COM2 is actually 'owning' the in-game volume)
* XMLVAR_COM_Transmit_Channel (1, 2 or 3)
