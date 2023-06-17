using Controlzmo.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.FlightControlUnit
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FcuSpeedData
    {
        [SimVar("AUTOPILOT MANAGED SPEED IN MACH", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int isMach;
        // This is the value actually shown on the FCU, even if it's a temporary selection.
        [SimVar("L:A32NX_AUTOPILOT_SPEED_SELECTED", "number", SIMCONNECT_DATATYPE.FLOAT64, 0.005f)]
        public double selectedSpeed;
        [SimVar("L:A32NX_FCU_SPD_MANAGED_DOT", "bool", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public int isManaged;
    };

    [Component]
    public class FcuSpeed : DataListener<FcuSpeedData>, IOnSimStarted, INotifyPropertyChanged
    {
        private FcuSpeedData current = new FcuSpeedData { isMach = 0, selectedSpeed = 100, isManaged = 0 };

        public bool IsManaged { get => current.isManaged == 1; }
        public bool IsDashes { get => current.selectedSpeed == -1; } // should match A32NX_FCU_SPD_MANAGED_DASHES
        public bool IsMach { get => current.isMach == 1; }
        public double DisplayedSpeed { get => current.selectedSpeed; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnStarted(ExtendedSimConnect simConnect) => simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SIM_FRAME);

        public override void Process(ExtendedSimConnect simConnect, FcuSpeedData data)
        {
            current = data;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FcuSpeedData"));
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
