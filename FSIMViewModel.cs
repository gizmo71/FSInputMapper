﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FSInputMapper.Systems.Fcu;
using FSInputMapper.Systems.Lights;

namespace FSInputMapper
{

    [Singleton]
    public class FSIMViewModel : INotifyPropertyChanged
    {

        public FSIMViewModel(DebugConsole debugConsole, LightSystem lightSystem, FcuSystem fcuSystem)
        {
            (this.debugConsole = debugConsole).PropertyChanged += OnDebugConsolePropertyChanged;
            (this.lightSystem = lightSystem).PropertyChanged += OnLightSystemPropertyChanged;
            (this.fcuSystem = fcuSystem).PropertyChanged += OnFcuSystemPropertyChanged;
        }

        #region Autopilot

        private readonly FcuSystem fcuSystem;

        private void OnFcuSystemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(sender == fcuSystem);
            switch (e.PropertyName)
            {
                case nameof(fcuSystem.Speed):
                    OnPropertyChange(nameof(AutopilotAirspeed));
                    break;
            }
        }

        public double AutopilotAirspeed
        {
            get { return fcuSystem.Speed; }
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

#endregion
#region Lights

        private readonly LightSystem lightSystem;

        private void OnLightSystemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(sender == lightSystem);
            switch (e.PropertyName)
            {
                case nameof(lightSystem.Strobes):
                    OnPropertyChange(nameof(StrobeLights));
                    break;
                case nameof(lightSystem.Beacon):
                    OnPropertyChange(nameof(BeaconLights));
                    break;
                case nameof(lightSystem.Wing):
                    OnPropertyChange(nameof(WingLights));
                    break;
                case nameof(lightSystem.NavLogo):
                    OnPropertyChange(nameof(NavLogoLights));
                    break;
                case nameof(lightSystem.RunwayTurnoff):
                    OnPropertyChange(nameof(RunwayTurnoffLights));
                    break;
                case nameof(lightSystem.Landing):
                    OnPropertyChange(nameof(LandingLights));
                    break;
                case nameof(lightSystem.Taxi0Off1Taxi2Takeoff):
                    OnPropertyChange(nameof(NoseLights));
                    break;
            }
        }

        public bool StrobeLights { get { return lightSystem.Strobes; } }
        public bool BeaconLights { get { return lightSystem.Beacon; } }
        public bool WingLights { get { return lightSystem.Wing; } }
        public bool NavLogoLights { get { return lightSystem.NavLogo; } }
        public bool RunwayTurnoffLights { get { return lightSystem.RunwayTurnoff; } }
        public bool LandingLights { get { return lightSystem.Landing; } }
        public int NoseLights { get { return 2 - lightSystem.Taxi0Off1Taxi2Takeoff; } }

#endregion
#region Debug

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

#endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
