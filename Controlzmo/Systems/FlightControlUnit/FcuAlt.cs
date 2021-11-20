using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Controlzmo.Systems.FlightControlUnit
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuAltData
    {
        [SimVar("AUTOPILOT ALTITUDE LOCK VAR:3", "feet", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int fcuAlt;
    };

    [Component]
    public class FcuAltListener : DataListener<FcuAltData>, IRequestDataOnOpen, INotifyPropertyChanged
    {
        public FcuAltData Current { get; private set; } = new FcuAltData { fcuAlt = 0 };
        public event PropertyChangedEventHandler? PropertyChanged;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuAltData data)
        {
            Current = data;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FcuAltData"));
        }
    }

    // The dot under Level Change
    [Component]
    public class FcuAltManaged : LVar, IOnSimStarted
    {
        public FcuAltManaged(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_FCU_ALT_MANAGED";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
        public bool IsManaged { get => Value == 1; }
    }

    [Component]
    public class FcuAltManagedSender : CreateOnStartup
    {
        private readonly SerialPico serial;
        private readonly FcuAltManaged fcuAltManaged;

        public FcuAltManagedSender(IServiceProvider serviceProvider)
        {
            serial = serviceProvider.GetRequiredService<SerialPico>();
            (fcuAltManaged = serviceProvider.GetRequiredService<FcuAltManaged>()).PropertyChanged += Regenerate;
        }

        private void Regenerate(object? _, PropertyChangedEventArgs? args) =>
            serial.SendLine("FcuAltManaged=" + fcuAltManaged.IsManaged);
    }

    [Component]
    public class FcuAltPulled : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;

        public FcuAltPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();
        public string SimEvent() => "A32NX.FCU_ALT_PULL";
        public string GetId() => "fcuAltPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuAltPushed : ISettable<bool>, IEvent
    {
        private readonly JetBridgeSender sender;

        public FcuAltPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();
        public string SimEvent() => "A32NX.FCU_ALT_PUSH";
        public string GetId() => "fcuAltPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuAltInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_INC";
    }

    [Component]
    public class FcultDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_ALT_DEC";
    }

    [Component]
    public class FcuAltDelta : ISettable<Int16>
    {
        private readonly FcuAltInc inc;
        private readonly FcultDec dec;

        public FcuAltDelta(IServiceProvider sp)
        {
            inc = sp.GetRequiredService<FcuAltInc>();
            dec = sp.GetRequiredService<FcultDec>();
        }

        public string GetId() => "fcuAltDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            while (value != 0)
            {
                simConnect.SendEvent(value < 0 ? dec : inc);
                value -= (short)Math.Sign(value);
            }
        }
    }

    [Component]
    public class FcuAltIncrement : ISettable<uint>, IEvent
    {
        public string GetId() => "fcuAltIncrement";
        public string SimEvent() => "A32NX.FCU_ALT_INCREMENT_SET";
        public void SetInSim(ExtendedSimConnect simConnect, uint value) => simConnect.SendEvent(this, value);
    }
}
