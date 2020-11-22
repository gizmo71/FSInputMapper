A320 FCU
========

Airspeed and Heading
--------------------

These are very similar to each other in behaviour.

There is currently no support for mach numbers.

Things to experiment with
-------------------------

According to a post on Discord, there are some more A/P variables at indexes:
```
A:AUTOPILOT ALTITUDE LOCK VAR:3 matched with K:2:AP_ALT_VAR_SET_ENGLISH - what is the "2"?
A:AUTOPILOT HEADING LOCK DIR:1
A:AUTOPILOT VERTICAL HOLD VAR:2
```
There may be a load more [here](https://github.com/Sequal32/yourcontrols/blob/master/definitions/aircraft/FBW%20A32NX%20Dev.yaml) including
```
AUTOPILOT FLIGHT DIRECTOR ACTIVE:1
AUTOPILOT FLIGHT DIRECTOR ACTIVE:2
TRANSPONDER STATE:1
TRANSPONDER CODE
```

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
Prepar3d has a bunch of other useful sounding KEY_ events listed.
```

In managed mode, the HDG-TRK and V/S-FPA numbers should show for 45 seconds
after selection until a timeout returns the display to dashes.
For Speed/Mach, it should timeout after 10 seconds.

References
----------

[Managed/Selected Mode Binding Thread](https://forums.flightsimulator.com/t/airbus-neo-is-there-a-binding-to-switch-between-managed-and-selected-modes/244977/15)
