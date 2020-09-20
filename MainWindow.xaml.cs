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
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.SPD_MAN);
        }

        private void Airspeed_Pull(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.SPD_SEL);
        }

        private void Faster10Knots(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAirspeed += 10;
        }

        private void Faster1Knot(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAirspeed += 1;
        }

        private void Slower10Knots(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAirspeed -= 10;
        }

        private void Slower1Knot(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAirspeed -= 1;
        }

        private void Heading_Push(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.HDG_MAN);
        }

        private void Heading_Pull(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.HDG_SEL);
        }

        private void Left10Degrees(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotHeading -= 10;
        }

        private void Left1Degree(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotHeading -= 1;
        }

        private void Right10Degrees(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotHeading += 10;
        }

        private void Right1Degree(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotHeading += 1;
        }

        private void Altitude_Push(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.ALT_MAN);
        }

        private void Altitude_Pull(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.ALT_SEL);
        }

        private void Up1000Feet(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAltitude += 1000;
        }

        private void Up100Feet(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAltitude += 100;
        }

        private void Down1000Feet(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAltitude -= 1000;
        }

        private void Down100Feet(object sender, RoutedEventArgs e)
        {
            _viewModel.AutopilotAltitude -= 100;
        }

    }
}
