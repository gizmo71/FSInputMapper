/*

A32NX_EFIS_{side}_ND_MODE

    Enum
    Provides the selected navigation display mode for the captain side EFIS
    Value 	Meaning
    0 	ROSE ILS
    1 	ROSE VOR
    2 	ROSE NAV
    3 	ARC
    4 	PLAN
    {side}
        L
        R

The navaid switches can be read/set using L:A32NX_EFIS_L/R_NAVAID_1/2_MODE (0 off, 1 ADF, 2 VOR).
The five option buttons can be read/set with L:A32NX_EFIS_L/R_OPTION (0 none, 1 CSTR, 3 WPT, 2 VORD, 4 NDB, 5 ARPT).

The LS pushbuttons under Baro are BTN_LS_1/2_FILTER_ACTIVE (read/write).
Where are the FD buttons?

*/