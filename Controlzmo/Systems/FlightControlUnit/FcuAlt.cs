using System;
using Controlzmo.Serial;
using Controlzmo.SimConnectzmo;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;

namespace Controlzmo.Systems.FlightControlUnit
{
    public class FcuAltMode : LVar, IOnSimConnection
    {
        private readonly SerialPico serial;

        public FcuAltMode(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
        }

        protected override string LVarName() => "A32NX_FCU_ALT_MANAGED";
        protected override int Milliseconds() => 167;
        protected override double Default() => -1.0;
        public void OnConnection(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set { base.Value = value; send(); } }

        private void send() => serial.SendLine(Value switch { 0 => "A", 1 => "a", _ => $"=Unknown FCU alt {Value}" });
    }
}
