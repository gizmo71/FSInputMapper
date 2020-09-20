﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FSInputMapper
{

    public class FSIMTrigger : EventArgs
    {
        public const string SPD_MAN = "Managed Speed";
        public const string SPD_SEL = "Selected Speed";
        public const string HDG_MAN = "Managed Heading";
        public const string HDG_SEL = "Selected Heading";
        public const string ALT_MAN = "Managed Altitude";
        public const string ALT_SEL = "Selected Altitude";
        public const string ALT_UP_100 = "Altitude +100";
        public const string ALT_UP_1000 = "Altitude +1000";
        public const string ALT_DOWN_100 = "Altitude -100";
        public const string ALT_DOWN_1000 = "Altitude -1000";

        public string? What { get; set; }
    }

    public class FSIMTriggerBus
    {
        public event EventHandler<FSIMTrigger> OnTrigger = delegate { };
        public void Trigger(object sender, string what)
        {
            OnTrigger(sender, new FSIMTrigger() { What = what });
        }
    }

}
