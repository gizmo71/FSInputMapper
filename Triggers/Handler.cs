namespace FSInputMapper
{

    //TODO: turn into different objects
    [Singleton]
    public class FSIMTriggerHandler
    {
        private readonly SimConnectHolder scHolder;
        private readonly FSIMViewModel viewModel; //TODO: remove

        public FSIMTriggerHandler(FSIMTriggerBus triggerBus, SimConnectHolder scHolder, FSIMViewModel viewModel)
        {
            triggerBus.OnTrigger += OnTrigger;
            this.scHolder = scHolder;
            this.viewModel = viewModel; //TODO: remove this and do those things another way
        }

        private void OnTrigger(object? sender, FSIMTriggerArgs e)
        {
            switch (e.What)
            {
                case FSIMTrigger.ALT_MAN:
                    scHolder.SimConnect?.SendEvent(EVENT.AP_ALTITUDE_SLOT_SET, 2u);
                    break;
                case FSIMTrigger.ALT_SEL:
                    scHolder.SimConnect?.SendEvent(EVENT.AP_ALTITUDE_SLOT_SET, 1u);
                    break;
                case FSIMTrigger.ALT_UP_1000:
                    scHolder.SimConnect?.SendEvent(EVENT.AP_ALT_UP, 1000u);
                    break;
                case FSIMTrigger.ALT_UP_100:
                    scHolder.SimConnect?.SendEvent(EVENT.AP_ALT_UP, 100u);
                    break;
                case FSIMTrigger.ALT_DOWN_1000:
                    scHolder.SimConnect?.SendEvent(EVENT.AP_ALT_DOWN, 1000u);
                    break;
                case FSIMTrigger.ALT_DOWN_100:
                    scHolder.SimConnect?.SendEvent(EVENT.AP_ALT_DOWN, 100u);
                    break;
                case FSIMTrigger.VS_STOP:
                    //TODO: A320_Neo_FCU.js says AP_PANEL_ALTITUDE_HOLD=1 and AP_PANEL_VS_ON=1
                    scHolder.SimConnect?.SendEvent(EVENT.FCU_VS_SET, 0);
                    goto vsSel;
                case FSIMTrigger.VS_SEL:
                //TODO: if in idle descent, set AP_VS_VAR_SET_ENGLISH twice with different parameters and the current rate, then send AP_PANEL_VS_ON=1
                //TODO: else, set AP_VS_VAR_SET_ENGLISH to current value, and then send VS_SLOT_INDEX_SET=AP_PANEL_VS_ON=1
                vsSel:
                    scHolder.SimConnect?.SendEvent(EVENT.FCU_VS_SLOT_SET, 1u);
                    goto vsPanelOn;
                case FSIMTrigger.VS_DOWN:
                    scHolder.SimConnect?.SendEvent(EVENT.FCU_VS_DOWN);
                    goto vsPanelOn;
                //TODO: if not yet active, on first turn set AP_VS_VAR_SET_ENGLISH starting value from VERTICAL SPEED
                //TODO: else, set AP_VS_VAR_SET_ENGLISH to new selection, and then send VS_SLOT_INDEX_SET=AP_PANEL_VS_ON=1
                case FSIMTrigger.VS_UP:
                    scHolder.SimConnect?.SendEvent(EVENT.FCU_VS_UP);
                vsPanelOn:
                    scHolder.SimConnect?.SendEvent(EVENT.AP_PANEL_VS_ON);
                    break;
                case FSIMTrigger.TOGGLE_LOC_MODE:
                    if (viewModel.AutopilotAppr)
                        scHolder.SimConnect?.SendEvent(EVENT.AP_TOGGLE_APPR);
                    scHolder.SimConnect?.SendEvent(EVENT.AP_TOGGLE_LOC);
                    break;
                case FSIMTrigger.TOGGLE_APPR_MODE:
                    if (viewModel.AutopilotLoc)
                        scHolder.SimConnect?.SendEvent(EVENT.AP_TOGGLE_LOC);
                    scHolder.SimConnect?.SendEvent(EVENT.AP_TOGGLE_APPR);
                    break;
            }
        }

    }

}
