using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FSInputMapper.Systems.Fcu;

namespace FSInputMapper
{

    [Singleton]
    public class FSIMViewModel : INotifyPropertyChanged
    {

        public FSIMViewModel(DebugConsole debugConsole, FcuSystem fcuSystem)
        {
            (this.debugConsole = debugConsole).PropertyChanged += OnDebugConsolePropertyChanged;
            (this.fcuSystem = fcuSystem).PropertyChanged += OnFcuSystemPropertyChanged;
        }
        #region Autopilot

        private readonly FcuSystem fcuSystem;

        private void OnFcuSystemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(sender == fcuSystem);
            switch (e.PropertyName)
            {
                case nameof(fcuSystem.Speed):
                    OnPropertyChange(nameof(AutopilotAirspeed));
                    break;
                case nameof(fcuSystem.SpeedSelected):
                    OnPropertyChange(nameof(AirspeedManaged));
                    break;
                case nameof(fcuSystem.Heading):
                    OnPropertyChange(nameof(AutopilotHeading));
                    break;
                case nameof(fcuSystem.HeadingSelected):
                    OnPropertyChange(nameof(HeadingManaged));
                    break;
            }
        }

        public double AutopilotAirspeed
        {
            get { return fcuSystem.Speed; }
        }

        public bool AirspeedManaged
        {
            get { return !fcuSystem.SpeedSelected; }
        }

        public int AutopilotHeading
        {
            get { return fcuSystem.Heading; }
        }

        public bool HeadingManaged
        {
            get { return !fcuSystem.HeadingSelected; }
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
        private void OnDebugConsolePropertyChanged(object? sender, PropertyChangedEventArgs e)
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
