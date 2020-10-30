using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace FSInputMapper.Systems.Lights
{

    [Singleton]
    public class LightSystem : INotifyPropertyChanged
    {

        private readonly SimConnectHolder scHolder;

        private bool wing;
        public bool Wing {
            get { return wing; }
            internal set { if (wing != value) { wing = value; OnPropertyChange(); } }
        }

        private bool beacon;
        public bool Beacon {
            get { return beacon; }
            internal set { if (beacon != value) { beacon = value; OnPropertyChange(); } }
        }

        private bool navLogo;
        public bool NavLogo {
            get { return navLogo; }
            internal set { if (navLogo != value) { navLogo = value; OnPropertyChange(); } }
        }

        private bool runwayTurnoff;
        public bool RunwayTurnoff {
            get { return runwayTurnoff; }
            internal set { if (runwayTurnoff != value) { runwayTurnoff = value; OnPropertyChange(); } }
        }

        private bool landing;
        public bool Landing
        {
            get { return landing; }
            internal set { if (landing != value) { landing = value; OnPropertyChange(); } }
        }

        private int taxi;
        public int Taxi0Off1Taxi2Takeoff
        {
            get { return taxi; }
            internal set { if (taxi != value) { taxi = value; OnPropertyChange(); } }
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

        internal void SetStrobes(bool? desired)
        {
            //TODO: store the desired state, then request states and set as required.
            // Our version of auto could include being on the runway.
//MessageBox.Show($"got {desired}", "SetStrobes", MessageBoxButton.OK, MessageBoxImage.Warning);
            if (desired != null)
                scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_STROBES_SET, desired == true ? 1u : 0u);
        }

        internal void SetBeacon(bool desired)
        {
            if (Beacon != desired)
                scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_BEACON_TOGGLE);
        }

        internal void SetWing(bool desired)
        {
            if (Wing != desired)
                scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_WING_TOGGLE);
        }

        internal void SetRunwayTurnoff(bool desired)
        {
            scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_TAXI_SET, desired ? 1u : 0u);
        }

        internal void SetLanding(bool desired)
        {
            scHolder.SimConnect?.SendEvent(EVENT.LIGHTS_LANDING_SET, desired ? 1u : 0u);
        }

        internal void SetNose(int off0Taxi1Landing2)
        {
            //TODO: hmm.
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
