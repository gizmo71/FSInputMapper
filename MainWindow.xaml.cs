using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Globalization;
using System.Windows.Controls;

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

        private void SpeedChange(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, (String)((Control)sender).Tag);
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
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.HDG_LEFT_10);
        }

        private void Left1Degree(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.HDG_LEFT_1);
        }

        private void Right10Degrees(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.HDG_RIGHT_10);
        }

        private void Right1Degree(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.HDG_RIGHT_1);
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
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.ALT_UP_1000);
        }

        private void Up100Feet(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.ALT_UP_100);
        }

        private void Down1000Feet(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.ALT_DOWN_1000);
        }

        private void Down100Feet(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.ALT_DOWN_100);
        }

        private void FcuLocClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.TOGGLE_LOC_MODE);
        }

        private void FcuApprClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.TOGGLE_APPR_MODE);
        }

        private void FcuGsClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerBus.Trigger(sender, FSIMTrigger.TOGGLE_GS_MODE);
        }

    }

}
