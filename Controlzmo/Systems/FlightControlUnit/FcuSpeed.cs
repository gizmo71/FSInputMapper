using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
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
    public class FcuSpeedMachToggled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_MACH_TOGGLE_PUSH";
        public string GetId() => "speedMachToggled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
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
    public class FcuSpeedPulled : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_PULL";
        public string GetId() => "fcuSpeedPulled";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuSpeedPushed : ISettable<bool>, IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_PUSH";
        public string GetId() => "fcuSpeedPushed";
        public void SetInSim(ExtendedSimConnect simConnect, bool _) => simConnect.SendEvent(this);
    }

    [Component]
    public class FcuSpeedInc : IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_INC";
    }

    [Component]
    public class FcuSpeedDec : IEvent
    {
        public string SimEvent() => "A32NX.FCU_SPD_DEC";
    }

    [Component]
    public class FcuSpeedDelta : ISettable<Int16>
    {
        private readonly FcuSpeedInc inc;
        private readonly FcuSpeedDec dec;

        public FcuSpeedDelta(IServiceProvider sp)
        {
            inc = sp.GetRequiredService<FcuSpeedInc>();
            dec = sp.GetRequiredService<FcuSpeedDec>();
        }

        public string GetId() => "fcuSpeedDelta";

        public void SetInSim(ExtendedSimConnect simConnect, Int16 value)
        {
            while (value != 0)
            {
                simConnect.SendEvent(value < 0 ? dec : inc);
                value -= (short)Math.Sign(value);
            }
        }
    }
}
