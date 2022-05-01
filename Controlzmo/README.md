Gizmo's Controlzmo
==================

Designed mostly to enhance interaction with the default A320neo in MSFS (aka FSFS2020).

When running, it will attempt to attached SimConnect to MSFS every couple of seconds until it succeeds, and should cope gracefully if it exits.

Lights
------

We should now be able to set the retraction state of the landing lights, and also do both independently, thought we would need to monitor LVars.

We could also split the landing lights into two, but it would be good to still be able to set them both at once easily - perhaps a "set for takeoff" button, including the nose light?

PM Calls
--------

* Can we get F/S speed calls during climb? A32NX_VSPEEDS_F/A32NX_VSPEEDS_S are listed as for "approach".
* Or perhaps warnings when we're about to exceed flap speeds during acceleration.
* Would it be possible to use speech recognition to ask for flap positions and have PM do the speed check first?

TODO
----

Calling OnConnection won't trigger an LVar update if SimConnect is already connected when UI demands initial state.
This is similar to the problem with "normal" data requiring an off/on flip.

* https://developer.mozilla.org/en-US/docs/Web/API/Gamepad_API/Using_the_Gamepad_API
* https://gamepad-tester.com/for-developers	

https://docs.microsoft.com/en-us/previous-versions/microsoft-esp/cc526953(v=msdn.10)?redirectedfrom=MSDN#theinfix2postfixtool

EFIS
----

* The LS pushbuttons under Baro are `L:BTN_LS_1/2_FILTER_ACTIVE` (read/write).
* Could really use a chrono button too (event A32NX.EFIS_L_CHRONO_PUSHED).

Other useful things?
--------------------

`A32NX_FWC_FLIGHT_PHASE` isn't 100% reliable; goes through 3 and 4 on the first run, but sometimes a second takeoff run goes straight from 2 to 8, especially if using FLEX.
See src/systems/systems/src/shared/mod.rs - 3 and 4 are the relevant ones.

Tiller seems to be mapped to `A:GEAR STEER ANGLE PCT` on ground, or `A:STEER INPUT CONTROL` in air.

Note that there's much better documentation on the RPN on the P3D site:
http://www.prepar3d.com/SDKv3/LearningCenter/utilities/scripting/rpn_scripting.html
