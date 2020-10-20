﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Globalization;
using System.Windows.Controls;

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

        public MainWindow(FSIMViewModel viewModel, SimConnectAdapter simConnectAdapter, FSIMTriggerBus triggerBus)
        {
            DataContext = viewModel;
            this.simConnectAdapter = simConnectAdapter;
            this.triggerBus = triggerBus;
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            simConnectAdapter.AttachWinow((HwndSource)PresentationSource.FromVisual(this));
        }

        private void Airspeed_Push(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.SPD_MAN);
        }

        private void Airspeed_Pull(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.SPD_SEL);
        }

        private void SpeedChange(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, (String)((Control)sender).Tag);
        }

        private void Heading_Push(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.HDG_MAN);
        }

        private void Heading_Pull(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.HDG_SEL);
        }

        private void Left10Degrees(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.HDG_LEFT_10);
        }

        private void Left1Degree(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.HDG_LEFT_1);
        }

        private void Right10Degrees(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.HDG_RIGHT_10);
        }

        private void Right1Degree(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.HDG_RIGHT_1);
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
            throw new Exception("Can't select GS alone");
        }

        private void BeaconLightsClicked(object sender, RoutedEventArgs e)
        {
            triggerBus.Trigger(sender, FSIMTrigger.LIGHTS_BEACON_TOGGLE);
        }

        private void StrobeLightsSelected(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                switch (cb.SelectedIndex)
                {
                    case 0:
                        triggerBus.Trigger(sender, FSIMTrigger.LIGHTS_STROBE_ON);
                        break;
                    case 2:
                        triggerBus.Trigger(sender, FSIMTrigger.LIGHTS_STROBE_OFF);
                        break;
                }
            }
        }
    }

}
