using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FSInputMapper.Systems.Fcu
{

    public partial class FcuSystem
    {

        public void SetSpeedSelected(bool isSelected)
        {
            scHolder.SimConnect?.SendEvent(EVENT.AP_SPEED_SLOT_SET, isSelected ? 1u : 2u);
        }

        private double speed;
        public double Speed
        {
            get { return speed; }
            internal set { if (speed != value) { speed = value; OnPropertyChange(); } }
        }

    }

}
