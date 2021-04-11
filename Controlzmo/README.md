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
* APU master/start (& bleed?) - `L:A32NX_APU_BLEED_AIR_VALVE_OPEN` (not settable),  lots of things with `L:*_OVHD_APU_*`
* States: `L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE`, `L:A32NX_OVHD_APU_START_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
* Master toggle: 0/`1 (>L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON)`; check with `(L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON, Bool)`
* Press start: `1 (>L:A32NX_OVHD_APU_START_PB_IS_ON`) but only when `(L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE, Bool)`
* Toggle bleed with 0/`1 (>L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`) Bool; also `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
