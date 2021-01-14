using System;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using FSInputMapper.Systems.Altimeter;
using FSInputMapper.Systems.Apu;
using FSInputMapper.Systems.Fcu;
using FSInputMapper.Systems.Lights;
using Microsoft.Extensions.DependencyInjection;

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

    [Singleton]
    public partial class MainWindow : Window
    {
        private readonly SimConnectAdapter simConnectAdapter;
        private readonly FSIMTriggerBus triggerBus;
        private readonly FcuSystem fcuSystem;
        private readonly ApuSystem apuSystem;
        private readonly ComRadioSystem comRadioSystem;

        public MainWindow(IServiceProvider sp)
        {
            DataContext = sp.GetRequiredService<FSIMViewModel>();
            this.simConnectAdapter = sp.GetRequiredService<SimConnectAdapter>();
            this.triggerBus = sp.GetRequiredService<FSIMTriggerBus>();
            this.fcuSystem = sp.GetRequiredService<FcuSystem>();
            this.apuSystem = sp.GetRequiredService<ApuSystem>();
            this.comRadioSystem = sp.GetRequiredService<ComRadioSystem>();
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            simConnectAdapter.AttachWindow((HwndSource)PresentationSource.FromVisual(this));
        }

        private void AirspeedManaged(object sender, RoutedEventArgs e)
        {
            fcuSystem.SetSpeedSelected(!(bool)((Control)sender).Tag);
        }

        private void SpeedChange(object sender, RoutedEventArgs e)
        {
            fcuSystem.SpeedChange((Int16)((Control)sender).Tag);
        }

        private void HeadingManaged(object sender, RoutedEventArgs e)
        {
            fcuSystem.SetHeadingSelected(!(bool)((Control)sender).Tag);
        }

        private void HeadingChange(object sender, RoutedEventArgs e)
        {
            fcuSystem.HeadingChange((Int16)((Control)sender).Tag);
        }

        private void Altitude_Push(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.ALT_MAN);
        }

        private void Altitude_Pull(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.ALT_SEL);
        }

        private void Up1000Feet(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.ALT_UP_1000);
        }

        private void Up100Feet(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.ALT_UP_100);
        }

        private void Down1000Feet(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.ALT_DOWN_1000);
        }

        private void Down100Feet(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.ALT_DOWN_100);
        }

        private void VSUp(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.VS_UP);
        }

        private void VSDown(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.VS_DOWN);
        }

        private void VS_Push(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.VS_STOP);
        }

        private void VS_Pull(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.VS_SEL);
        }

        private void FcuLocClicked(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.TOGGLE_LOC_MODE);
        }

        private void FcuApprClicked(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.TOGGLE_APPR_MODE);
        }

        private void FcuGsClicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Can't select GS alone", "Please do not press that button again",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ApuMaster(object sender, RoutedEventArgs e)
        {
            apuSystem.ApuToggle();
        }

        private void ApuStart(object sender, RoutedEventArgs e)
        {
            apuSystem.ApuStart();
        }

        private void Com1Swap(object sender, RoutedEventArgs args)
        {
            Decimal newFreq = (DataContext as FSIMViewModel)!.Com1StandbyFrequency;
            try
            {
                comRadioSystem.SetCom1Standby(newFreq);
                comRadioSystem.SwapCom1();
                //TODO: swapped out frequency
                //(DataContext as FSIMViewModel)!.Com1StandbyFrequency = new Decimal;
            }
            catch (Exception)
            {
                (DataContext as FSIMViewModel)!.Com1StandbyFrequency = Decimal.Zero;
            }
        }
    }
}
