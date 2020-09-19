A320 FCU
========

Airspeed and Heading
--------------------

These are very similar to each other in behaviour.

There is currently no support for mach numbers.

Things to experiment with
-------------------------

```
  * Set ''K:SPEED_SLOT_INDEX_SET'' with a value of 1 (selected) or 2 (managed); variable ''AUTOPILOT_SPEED_SLOT_INDEX'' to read it (don't use underscores!)
  * Set ''L:A320_FCU_SHOW_SELECTED_SPEED'' to show a selected speed number, or 0 for dashes (can't read from SimConnect, at least not the same way as the slot above)
  * Similarly, ''K:HEADING_SLOT_INDEX_SET'' is 1 (selected) or 2 (managed)
  * ''L:A320_FCU_SHOW_SELECTED_HEADING'' for the selected heading shown
  * Altitude version involves ''L:A320_NEO_FCU_FORCE_IDLE_VS'', ''K:ALTITUDE_SLOT_INDEX_SET'', something internal called ''AP_ALT_VAR_SET_ENGLISH''
For more, look at the JavaScript files in ''asobo-vcockpits-instruments-a320-neo/html_ui/Pages/VCockpit/Instruments/Airliners/A320_Neo''.
```

References
----------

https://forums.flightsimulator.com/t/airbus-neo-is-there-a-binding-to-switch-between-managed-and-selected-modes/244977/15|this]]
