using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FSInputMapper
{
    public class FSIMViewModel : INotifyPropertyChanged
    {
        private Int32 altitude = 0;
        public Int32 Altitude
        {
            get { return altitude; }
            set { if (altitude != value) { altitude = value; OnPropertyChange(); } }
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
