A320 FCU
========

Airspeed and Heading
--------------------

These are very similar to each other in behaviour.

There is currently no support for mach numbers.

Things to experiment with
-------------------------

```
There are also "SET AP MANAGED SPEED IN MACH"/" ON"/" OFF" - contradiction in terms?!
"SET AUTOPILOT AIRSPEED HOLD"? "SET AUTOPILOT MACH HOLD"? "SET AUTOPILOT MACH REFERENCE"?
```
```
Set ''K:SPEED_SLOT_INDEX_SET'' with a value of 1 (selected) or 2 (managed); variable ''AUTOPILOT_SPEED_SLOT_INDEX'' to read it (don't use underscores!)
Set ''L:A320_FCU_SHOW_SELECTED_SPEED'' to show a selected speed number, or 0 for dashes (can't read from SimConnect, at least not the same way as the slot above)
If AUTOPILOT GLIDESLOPE HOLD is on, don't show selected speed.
Similarly, ''K:HEADING_SLOT_INDEX_SET'' is 1 (selected) or 2 (managed)
''L:A320_FCU_SHOW_SELECTED_HEADING'' for the selected heading shown
Altitude version involves ''L:A320_NEO_FCU_FORCE_IDLE_VS'', ''K:ALTITUDE_SLOT_INDEX_SET'', something internal called ''AP_ALT_VAR_SET_ENGLISH''
TODO: may also need to send FLIGHT_LEVEL_CHANGE_ON when setting alt managed or when trying to get manual control of V/S.
For more, look at the JavaScript files in ''asobo-vcockpits-instruments-a320-neo/html_ui/Pages/VCockpit/Instruments/Airliners/A320_Neo''.
Vertical speed stuff:
Note that there's lots of stuff in the JS which isn't event driven, so you might have to replicate timeouts etc.
TODO: "SET AP CURRENT VS"? "SET AUTOPILOT VS HOLD"?
Also the selection increment.
Prepar3d has a bunch of other useful sounding KEY_ events listed.
On pushing the button to level off, triggers K:AP_PANEL_ALTITUDE_HOLD and K:AP_PANEL_VS_ON [A320_Neo_FCU.js]
On turning, sends/sets AP_VS_VAR_SET_ENGLISH to 2 if in idle descent;
  sends AP_VS_VAR_SET_ENGLISH with the value and sets K:VS_SLOT_INDEX_SET=K:AP_PANEL_VS_ON=1 if 'normal' or levelling off
On pulling, sends AP_VS_VAR_SET_ENGLISH to the desired value, and sets K:VS_SLOT_INDEX_SET=K:AP_PANEL_VS_ON=1 if at idle descent;
  sets L:A320_NEO_FCU_FORCE_SELECTED_ALT=1, sets AP_VS_VAR_SET_ENGLISH twice with different parameters, and sets K:AP_PANEL_VS_ON=1
```
Add vertical speed, `AUTOPILOT VERTICAL HOLD VAR` in "feet/minute",
and maybe `AUTOPILOT VERTICAL HOLD` and even `AUTOPILOT_VS_SLOT_INDEX`.
```
Selecting bugs: Shift+Control+r (airspeed) z (altitude) h (heading) ?? (VSI)
```

In managed mode, the HDG-TRK and V/S-FPA numbers should show for 45 seconds
after selection until a timeout returns the display to dashes.
For Speed/Mach, it should timeout after 10 seconds.

References
----------

[Managed/Selected Mode Binding Thread](https://forums.flightsimulator.com/t/airbus-neo-is-there-a-binding-to-switch-between-managed-and-selected-modes/244977/15)
