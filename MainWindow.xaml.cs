using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Globalization;

// https://docs.microsoft.com/en-us/dotnet/desktop/wpf/getting-started/walkthrough-my-first-wpf-desktop-application?view=netframeworkdesktop-4.8

namespace FSInputMapper
{
    public class ModeBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value == (bool)parameter ? "Green" : "Transparent";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        private readonly FSIMViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new FSIMViewModel();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _ = new SimConnectAdapter((HwndSource)PresentationSource.FromVisual(this), _viewModel);
        }

        private void Airspeed_Push(object sender, RoutedEventArgs e)
        {
            _viewModel.AirspeedManaged = true;
        }

        private void Airspeed_Pull(object sender, RoutedEventArgs e)
        {
            _viewModel.AirspeedManaged = false;
        }

        private void Heading_Push(object sender, RoutedEventArgs e)
        {
            _viewModel.HeadingManaged = true;
        }

        private void Heading_Pull(object sender, RoutedEventArgs e)
        {
            _viewModel.HeadingManaged = false;
        }

        private void Altitude_Push(object sender, RoutedEventArgs e)
        {
            _viewModel.AltitudeManaged = true;
        }

        private void Altitude_Pull(object sender, RoutedEventArgs e)
        {
            _viewModel.AltitudeManaged = false;
        }

        private void Up1000_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAltitude += 1000;
        }

        private void Up100_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAltitude += 100;
        }

        private void Down1000_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAltitude -= 1000;
        }

        private void Down100_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAltitude -= 100;
        }

    }
}
