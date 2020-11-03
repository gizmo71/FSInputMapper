﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Globalization;
using System.Windows.Controls;
using FSInputMapper.Systems.Lights;
using FSInputMapper.Systems.Fcu;
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
        private readonly LightSystem lightSystem;
        private readonly FcuSystem fcuSystem;

        public MainWindow(IServiceProvider sp)
        {
            DataContext = sp.GetRequiredService<FSIMViewModel>();
            this.simConnectAdapter = sp.GetRequiredService<SimConnectAdapter>();
            this.triggerBus = sp.GetRequiredService<FSIMTriggerBus>();
            this.lightSystem = sp.GetRequiredService<LightSystem>();
            this.fcuSystem = sp.GetRequiredService<FcuSystem>();
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            simConnectAdapter.AttachWinow((HwndSource)PresentationSource.FromVisual(this));
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
            fcuSystem.SpeedChange((Int16)((Control)sender).Tag);
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

        private void StrobeLightsClicked(object sender, RoutedEventArgs e)
        {
            lightSystem.SetStrobes((sender as CheckBox)!.IsChecked ?? false);
        }

        private void BeaconLightsClicked(object sender, RoutedEventArgs e)
        {
            lightSystem.SetBeacon((sender as CheckBox)!.IsChecked ?? false);
        }

        private void WingLightsClicked(object sender, RoutedEventArgs e)
        {
            lightSystem.SetWing((sender as CheckBox)!.IsChecked ?? false);
        }

        private void NavLogoLightsClicked(object sender, RoutedEventArgs e)
        {
            lightSystem.SetNavLogo((sender as CheckBox)!.IsChecked ?? false);
        }

        private void RunwayTurnoffLightsClicked(object sender, RoutedEventArgs e)
        {
            lightSystem.SetRunwayTurnoff((sender as CheckBox)!.IsChecked ?? false);
        }

        private void LandingLightsClicked(object sender, RoutedEventArgs e)
        {
            lightSystem.SetLanding((sender as CheckBox)!.IsChecked ?? false);
        }

    }

}
