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

On landing, would be good to detect spoilers popping (see A320_Neo_LowerECAM_FTCL.js), reversers, and decel light.
```this.flightPhase = SimVar.GetSimVarValue("L:A32NX_FMGC_FLIGHT_PHASE", "number"); - takeoff is 1
this.autoBreakLevel = SimVar.GetSimVarValue("L:XMLVAR_Autobrakes_Level", "Enum");
this.autoBreakDecel = this.autoBreakLevel !== 0 && this.isOnGround && (this.autoBreakLevel === 3 ? -6 : -2) > SimVar.GetSimVarValue("A:ACCELERATION BODY Z", "feet per second squared");
this.autoBreakActivated = SimVar.GetSimVarValue("L:A32NX_AUTOBRAKES_BRAKING", "Bool") === 1;```

https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-simvars.md
https://github.com/flybywiresim/a32nx/blob/autopilot/docs/a320-events.md

APU controls - not part of PM - A32NX_APU_BLEED_AIR_VALVE_OPEN
