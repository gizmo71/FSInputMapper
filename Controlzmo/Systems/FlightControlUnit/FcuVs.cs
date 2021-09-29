using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using SimConnectzmo;
using System;

/*TODO: Display stuff.
A32NX_AUTOPILOT_FPA_SELECTED or A32NX_AUTOPILOT_VS_SELECTED - this is the number shown.
    How do we know when to show dashes?
    A320_NE0_FCU_STATE (seems to be 0 when in ALT, 1 when pushed to level, 2 for selecting V/S, and 3 after pulling)
    https://github.com/flybywiresim/a32nx/blob/42e4134f9235ff0a1842edc10aad7c56a52b7989/flybywire-aircraft-a320-neo/html_ui/Pages/VCockpit/Instruments/Airliners/FlyByWire_A320_Neo/FCU/A320_Neo_FCU.js
Choice based on A32NX_TRK_FPA_MODE_ACTIVE: 0 for HDG+V/S, 1 for TRK+FPA.
A32NX_FCU_VS_MANAGED is just the dot under Level Change.*/
namespace Controlzmo.Systems.FlightControlUnit
{
    [Component]
    public class FcuVsMode : LVar, IOnSimStarted
    {
        private readonly SerialPico serial;

        public FcuVsMode(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
        }

        protected override string LVarName() => "A32NX_FCU_VS_MANAGED";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);

        protected override double? Value { set { base.Value = value; send(); } }

        private void send() => serial.SendLine("FcuVsManaged=" + (Value == 1));
    }

    [Component]
    public class FcuVsPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuVsPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuVsPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_VS_PULL)");
    }

    [Component]
    public class FcuVsPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuVsPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuVsPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_VS_PUSH)");
    }

    [Component]
    public class FcuVsDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuVsDelta(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuVsDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_VS_DEC" : "A32NX.FCU_VS_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short)Math.Sign(value);
            }
        }
    }
}
