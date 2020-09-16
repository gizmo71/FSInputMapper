using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FSInputMapper
{
    public class FSIMViewModel : INotifyPropertyChanged
    {
        private Int32 apAirspeed = 100;
        public Int32 AutopilotAirspeed
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

        bool altitudeManaged = true;
        public bool AltitudeManaged
        {
            get { return altitudeManaged; }
            set { if (altitudeManaged != value) { altitudeManaged = value; OnPropertyChange(); } }
        }

        private string? connectionError = "Not yet connected";
        public string? ConnectionError
        {
            get { return connectionError; }
            set { if (connectionError != value) { connectionError = value; OnPropertyChange(); OnPropertyChange(nameof(IsConnected)); } }
        }
        public bool IsConnected
        {
            get { return connectionError == null; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
