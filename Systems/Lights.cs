using System;
using System.Collections.Generic;
using System.Text;

namespace FSInputMapper.Systems
{

    [Singleton]
    public class LightSystem
    {

        private readonly SimConnectHolder scHolder;

        public LightSystem(SimConnectHolder scHolder)
        {
            this.scHolder = scHolder;
            /*TODO: stuff
             * Instructions from UI/HW can come as simple method calls.
             * That should include the initial state where required (e.g. hardware switches).
             * Updates back the other way need to be done via events of some sort.
             */
        }

        internal void SetNavLogo(bool desired)
        {
            uint data = desired ? 1u : 0u;
            scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_NAV_SET, data);
            scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_LOGO_SET, data);
        }

    }

}
