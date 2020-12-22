using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace FSInputMapper.Systems.Lights
{

    [Singleton]
    public class LightSystem : INotifyPropertyChanged
    {

        private readonly SimConnectHolder scHolder;
        private readonly SetStrobesEvent setStrobesEvent;
        private readonly ToggleBeaconLightsEvent toggleBeaconLightsEvent;
        private readonly ToggleWingIceLightsEvent toggleWingIceLightsEvent;
        private readonly SetNavLightsEvent setNavLightsEvent;
        private readonly SetLogoLightsEvent setLogoLightsEvent;
        private readonly SetTaxiLightsEvent setTaxiLightsEvent;
        private readonly SetLandingLightsEvent setLandingLightsEvent;

        public LightSystem(IServiceProvider sp)
        {
            this.scHolder = sp.GetRequiredService<SimConnectHolder>();
            this.setStrobesEvent = sp.GetRequiredService<SetStrobesEvent>();
            this.toggleBeaconLightsEvent = sp.GetRequiredService<ToggleBeaconLightsEvent>();
            this.toggleWingIceLightsEvent = sp.GetRequiredService<ToggleWingIceLightsEvent>();
            this.setNavLightsEvent = sp.GetRequiredService<SetNavLightsEvent>();
            this.setLogoLightsEvent = sp.GetRequiredService<SetLogoLightsEvent>();
            this.setTaxiLightsEvent = sp.GetRequiredService<SetTaxiLightsEvent>();
            this.setLandingLightsEvent = sp.GetRequiredService<SetLandingLightsEvent>();
        }

        private bool strobes;
        public bool Strobes
        {
            get { return strobes; }
            internal set { if (strobes != value) { strobes = value; OnPropertyChange(); } }
        }

        private bool isStrobeAuto;
        public bool IsStrobeAuto
        {
            get { return isStrobeAuto; }
            internal set { if (isStrobeAuto != value) { isStrobeAuto = value; OnPropertyChange(); } }
        }

        private bool wing;
        public bool Wing
        {
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

        internal void SetNavLogo(bool desired)
        {
            uint data = desired ? 1u : 0u;
            scHolder.SimConnect?.SendEvent(setNavLightsEvent, data);
            scHolder.SimConnect?.SendEvent(setLogoLightsEvent, data);
        }

        internal void SetStrobes(bool desired)
        {
            scHolder.SimConnect?.SendEvent(setStrobesEvent, desired ? 1u : 0u);
        }

        internal void SetBeacon(bool desired)
        {
            if (Beacon != desired)
                scHolder.SimConnect?.SendEvent(toggleBeaconLightsEvent);
        }

        internal void SetWing(bool desired)
        {
            if (Wing != desired)
                scHolder.SimConnect?.SendEvent(toggleWingIceLightsEvent);
        }

        internal void SetRunwayTurnoff(bool desired)
        {
            scHolder.SimConnect?.SendEvent(setTaxiLightsEvent, desired ? 1u : 0u);
        }

        internal void SetLanding(bool desired)
        {
            scHolder.SimConnect?.SendEvent(setLandingLightsEvent, desired ? 1u : 0u);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
