Gizmo's FSInputMapper
=====================

Designed mostly to enhance interaction with the default A320neo in MSFS (aka FSFS2020).

Spoiler Controls
----------------

Inside MSFS, the user should map the "TOGGLE SPOILERS" and "TOGGLE ARM SPOILERS" to two inputs,
which allows graceful degredation of the behaviour if FSInputMapper isn't running.

When it is, the former command will disarm the spoilers if they are armed, or increase speedbrakes by 25% if not;
the latter command will decreased the speedbrake position by 25%, or arm the spoilers if the speedbrakes were fully retracted.

In effect, this yields six positions (armed, retracted, 25%, 50%, 75%, fully extended)
and allows the user to move through that sequence in either direction.
