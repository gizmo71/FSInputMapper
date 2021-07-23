Gizmo's Controlzmo
==================

Designed mostly to enhance interaction with the default A320neo in MSFS (aka FSFS2020).

When running, it will attempt to attached SimConnect to MSFS every couple of seconds until it succeeds, and should cope gracefully if it exits.

Lights
------

TODO

PM Calls
--------

* `A32NX_AUTOTHRUST_MODE_MESSAGE` value `3` means "LVR CLB" - have PM annunciate?
* `A32NX_BRAKES_HOT`

TODO
----

https://developer.mozilla.org/en-US/docs/Web/API/Gamepad_API/Using_the_Gamepad_API

https://docs.microsoft.com/en-us/previous-versions/microsoft-esp/cc526953(v=msdn.10)?redirectedfrom=MSDN#theinfix2postfixtool

Other useful things?

`A32NX_FWC_FLIGHT_PHASE` isn't 100% reliable; goes through 3 and 4 on the first run, but sometimes a second takeoff run goes straight from 2 to 8, especially if using FLEX.
See src/systems/systems/src/shared/mod.rs - 3 and 4 are the relevant ones.

Tiller seems to be mapped to `A:GEAR STEER ANGLE PCT` on ground, or `A:STEER INPUT CONTROL` in air.

APU:
* States: `L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE`, `L:A32NX_OVHD_APU_START_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`, `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
* Master toggle: 0/`1 (>L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON)`; check with `(L:A32NX_OVHD_APU_MASTER_SW_PB_IS_ON, Bool)`
* Press start: `1 (>L:A32NX_OVHD_APU_START_PB_IS_ON`) but only when `(L:A32NX_OVHD_APU_START_PB_IS_AVAILABLE, Bool)`
* Toggle bleed with 0/`1 (>L:A32NX_OVHD_PNEU_APU_BLEED_PB_IS_ON`) Bool; also `L:A32NX_OVHD_PNEU_APU_BLEED_PB_HAS_FAULT`
