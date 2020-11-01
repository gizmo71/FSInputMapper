using System;
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
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChange([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
