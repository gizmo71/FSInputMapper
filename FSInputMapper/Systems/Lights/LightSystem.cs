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
        private readonly ToggleWingIceLightsEvent toggleWingIceLightsEvent;
        private readonly SetNavLightsEvent setNavLightsEvent;
        private readonly SetLogoLightsEvent setLogoLightsEvent;
        private readonly SetTaxiLightsEvent setTaxiLightsEvent;

        public LightSystem(IServiceProvider sp)
        {
            this.scHolder = sp.GetRequiredService<SimConnectHolder>();
            this.toggleWingIceLightsEvent = sp.GetRequiredService<ToggleWingIceLightsEvent>();
            this.setNavLightsEvent = sp.GetRequiredService<SetNavLightsEvent>();
            this.setLogoLightsEvent = sp.GetRequiredService<SetLogoLightsEvent>();
            this.setTaxiLightsEvent = sp.GetRequiredService<SetTaxiLightsEvent>();
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

        internal void SetWing(bool desired)
        {
            if (Wing != desired)
                scHolder.SimConnect?.SendEvent(toggleWingIceLightsEvent);
        }

        internal void SetRunwayTurnoff(bool desired)
        {
            scHolder.SimConnect?.SendEvent(setTaxiLightsEvent, desired ? 1u : 0u);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
