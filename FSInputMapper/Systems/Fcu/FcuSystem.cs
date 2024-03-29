﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace FSInputMapper.Systems.Fcu
{

    [Singleton]
    public partial class FcuSystem
    {

        private readonly SimConnectHolder scHolder;

        public FcuSystem(IServiceProvider sp)
        {
            this.scHolder = sp.GetRequiredService<SimConnectHolder>();
            #region Speed
            this.speedSelectedListener = sp.GetRequiredService<FcuSpeedSelectedListener>();
            this.speedModeSet = sp.GetRequiredService<FcuSpeedModeSet>();
            this.speedIncrease = sp.GetRequiredService<FcuSpeedIncrease>();
            this.speedDecrease = sp.GetRequiredService<FcuSpeedDecrease>();
            #endregion
            #region Heading
            this.headingSelectedListener = sp.GetRequiredService<FcuHeadingSelectedListener>();
            this.headingModeSet = sp.GetRequiredService<FcuHeadingModeSet>();
            this.headingDecrease = sp.GetRequiredService<FcuHeadingDecrease>();
            this.headingIncrease = sp.GetRequiredService<FcuHeadingIncrease>();
            #endregion
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
