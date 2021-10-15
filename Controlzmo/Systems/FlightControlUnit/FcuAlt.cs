using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger logging;

        public FcuAltListener(IServiceProvider sp)
        {
            logging = sp.GetRequiredService<ILogger<FcuAltListener>>();
        }

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
    public class FcuAltPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuAltPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuAltPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool _ )
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_ALT_PULL)");
    }

    [Component]
    public class FcuAltPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuAltPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuAltPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool _)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_ALT_PUSH)");
    }

    [Component]
    public class FcuAltDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuAltDelta(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuAltDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_ALT_DEC" : "A32NX.FCU_ALT_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short)Math.Sign(value);
            }
        }
    }

    [Component]
    public class FcuAltIncrement : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuAltIncrement(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuAltIncrement";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            sender.Execute(simConnect, $"{value} (>K:A32NX.FCU_ALT_INCREMENT_SET)");
        }
    }
}
