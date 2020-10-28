using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Text;

namespace FSInputMapper.Systems.Lights
{

    [Singleton]
    public class LightSystem : INotifyPropertyChanged
    {

        private readonly SimConnectHolder scHolder;

        private bool wingLights;
        public bool WingLights {
            get { return wingLights; }
            internal set { if (wingLights != value) { wingLights = value; OnPropertyChange(); } }
        }

        private bool beaconLights;
        public bool BeaconLights {
            get { return beaconLights; }
            internal set { if (beaconLights != value) { beaconLights = value; OnPropertyChange(); } }
        }

        public LightSystem(SimConnectHolder scHolder)
        {
            this.scHolder = scHolder;
        }

        internal void SetNavLogo(bool desired)
        {
            uint data = desired ? 1u : 0u;
            scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_NAV_SET, data);
            scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_LOGO_SET, data);
        }

        internal void SetWing(bool desired)
        {
//TODO: check against actual state?
            scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_WING_TOGGLE);
        }

        internal void SetBeacon(bool desired)
        {
//TODO: check against actual state?
            scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_BEACON_TOGGLE);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

}

}
