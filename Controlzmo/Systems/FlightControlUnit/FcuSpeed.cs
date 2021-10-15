using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Controlzmo.Systems.JetBridge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.FlightControlUnit
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuSpeedMachData
    {
        [SimVar("AUTOPILOT MANAGED SPEED IN MACH", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int isMach;
    };

    [Component]
    public class FcuSpeedMachListener : DataListener<FcuSpeedMachData>, IRequestDataOnOpen, INotifyPropertyChanged
    {
        private FcuSpeedMachData current = new FcuSpeedMachData { isMach = 0 };

        public bool IsMach { get => current.isMach != 0; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, FcuSpeedMachData data)
        {
            current = data;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FcuSpeedMachData"));
        }
    }

    [Component]
    public class FcuSpeedManaged : LVar, IOnSimStarted
    {
        public FcuSpeedManaged(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_FCU_SPD_MANAGED_DOT";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
        public bool IsManaged { get => Value == 1; }
    }

    [Component]
    // This is the value actually shown on the FCU, even if it's a temporary selection.
    public class FcuSpeedSelection : LVar, IOnSimStarted
    {
        public FcuSpeedSelection(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOPILOT_SPEED_SELECTED";
        public void OnStarted(ExtendedSimConnect simConnect) => Request(simConnect);
        public bool IsDashes { get => Value == -1; } // should match A32NX_FCU_SPD_MANAGED_DASHES
        public bool IsMach { get => Value > 0 && Value < 10; } // 0.10 to 0.99
        public bool IsKnots { get => Value >= 100 && Value < 1000; } // 100 to 399
    }

    [Component]
    public class FcuSpeedPulled : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuSpeedPulled(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuSpeedPulled";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_SPD_PULL)");
    }

    [Component]
    public class FcuSpeedPushed : ISettable<bool>
    {
        private readonly JetBridgeSender sender;

        public FcuSpeedPushed(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuSpeedPushed";

        public void SetInSim(ExtendedSimConnect simConnect, bool value)
            => sender.Execute(simConnect, $"(>K:A32NX.FCU_SPD_PUSH)");
    }

    [Component]
    public class FcuSpeedDelta : ISettable<Int16>
    {
        private readonly JetBridgeSender sender;

        public FcuSpeedDelta(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "fcuSpeedDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            var eventCode = value < 0 ? "A32NX.FCU_SPD_DEC" : "A32NX.FCU_SPD_INC";
            while (value != 0)
            {
                sender.Execute(simConnect, $"0 (>K:{eventCode})");
                value -= (short)Math.Sign(value);
            }
        }
    }
}
