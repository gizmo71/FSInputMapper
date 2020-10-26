using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FSInputMapper
{

    [Singleton]
    public class FSIMViewModel : INotifyPropertyChanged
    {

        public FSIMViewModel(DebugConsole debugConsole)
        {
            (this.debugConsole = debugConsole).PropertyChanged += OnDebugConsolePropertyChanged;
        }

        private double apAirspeed = 100.0;
        public double AutopilotAirspeed
        {
            get { return apAirspeed; }
            set { if (apAirspeed != value) { apAirspeed = value; OnPropertyChange(); } }
        }

        bool airspeedManaged = true;
        public bool AirspeedManaged
        {
            get { return airspeedManaged; }
            set { if (airspeedManaged != value) { airspeedManaged = value; OnPropertyChange(); } }
        }

        private Int32 apHeading = 0;
        public Int32 AutopilotHeading
        {
            get { return apHeading; }
            set { if (apHeading != value) { apHeading = value; OnPropertyChange(); } }
        }

        bool headingManaged = true;
        public bool HeadingManaged
        {
            get { return headingManaged; }
            set { if (headingManaged != value) { headingManaged = value; OnPropertyChange(); } }
        }

        private Int32 apAltitude = 5000;
        public Int32 AutopilotAltitude
        {
            get { return apAltitude; }
            set { if (apAltitude != value) { apAltitude = value; OnPropertyChange(); } }
        }

        private bool altitudeManaged = true;
        public bool AltitudeManaged
        {
            get { return altitudeManaged; }
            set { if (altitudeManaged != value) { altitudeManaged = value; OnPropertyChange(); } }
        }

        private Int32 autopilotVerticalSpeed = 0;
        public Int32 AutopilotVerticalSpeed
        {
            get { return autopilotVerticalSpeed; }
            set { if (autopilotVerticalSpeed != value) { autopilotVerticalSpeed = value; OnPropertyChange(); } }
        }

        private bool vsManaged = true;
        public bool VerticalSpeedManaged
        {
            get { return vsManaged; }
            set { if (vsManaged != value) { vsManaged = value; OnPropertyChange(); } }
        }

        private bool autopilotLoc;
        public bool AutopilotLoc {
            get { return autopilotLoc; }
            set { if (autopilotLoc != value) { autopilotLoc = value; OnPropertyChange(); } }
        }

        private bool autopilotGs;
        public bool AutopilotGs
        {
            get { return autopilotGs; }
            set { if (autopilotGs != value) { autopilotGs = value; OnPropertyChange(); } }
        }

        private bool autopilotAppr;
        public bool AutopilotAppr
        {
            get { return autopilotAppr; }
            set { if (autopilotAppr != value) { autopilotAppr = value; OnPropertyChange(); } }
        }

        private int strobes = 1;
        public int Strobes
        {
            get { return strobes; }
            set { if (strobes != value) { strobes = value; OnPropertyChange(); } }
        }

        private bool beaconLights;
        public bool BeaconLights
        {
            get { return beaconLights; }
            set { if (beaconLights != value) { beaconLights = value; OnPropertyChange(); } }
        }

        private bool wingLights;
        public bool WingLights
        {
            get { return wingLights; }
            set { if (wingLights != value) { wingLights = value; OnPropertyChange(); } }
        }

        private bool navLogoLights;
        public bool NavLogoLights
        {
            get { return navLogoLights; }
            set { if (navLogoLights != value) { navLogoLights = value; OnPropertyChange(); } }
        }

        private bool runwayTurnoffLights;
        public bool RunwayTurnoffLights
        {
            get { return runwayTurnoffLights; }
            set { if (runwayTurnoffLights != value) { runwayTurnoffLights = value; OnPropertyChange(); } }
        }

        private int noseLights;
        public int NoseLights
        {
            get { return noseLights; }
            set { if (noseLights != value) { noseLights = value; OnPropertyChange(); } }
        }

        private int landingLights = 1;
        public int LandingLights
        {
            get { return landingLights; }
            set { if (landingLights != value) { landingLights = value; OnPropertyChange(); } }
        }

        private readonly DebugConsole debugConsole;

        public string? ConnectionError
        {
            get { return debugConsole.ConnectionError; }
        }
        public bool IsConnected
        {
            get { return debugConsole.ConnectionError == null; }
        }

        public string DebugConsoleText { get { return debugConsole.Text; } }
        private void OnDebugConsolePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(sender == debugConsole);
            switch (e.PropertyName) {
                case nameof(debugConsole.Text):
                    OnPropertyChange(nameof(DebugConsoleText));
                    break;
                case nameof(debugConsole.ConnectionError):
                    OnPropertyChange(nameof(ConnectionError));
                    OnPropertyChange(nameof(IsConnected));
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
