using System;
using System.Collections.Generic;
using System.Text;

namespace FSInputMapper
{

    public class FSIMTrigger : EventArgs
    {
        public string? What { get; set; }
    }

    public class FSIMTriggerBus
    {
        public event EventHandler<FSIMTrigger> OnTrigger = delegate { };
        public void Trigger(object sender)
        {
            OnTrigger(this, new FSIMTrigger() { What = "foo!" });
        }
    }

}
