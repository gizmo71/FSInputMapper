using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Controls.Engine
{
    public interface IEngineFireFaultLightData
    {
        public Int32 IsOn { get; }
    }

    [RequiredArgsConstructor]
    public abstract partial class EngineFireFaultLight<T> : DataListener<T>, IRequestDataOnOpen where T : struct, IEngineFireFaultLightData
    {
        private readonly UrsaMinorOutputs outputs;
        private readonly int engine;
        private readonly bool isFire;
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;
        public override void Process(ExtendedSimConnect simConnect, T data) => outputs.SetEngineWarning(engine, isFire, data.IsOn != 0);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct Engine1FaultLightData : IEngineFireFaultLightData
    {
        [Property]
        [SimVar("L:I_ENG_FAULT_1", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 _isOn;
    }

    [Component, RequiredArgsConstructor]
    public partial class Engine1FaultLight : EngineFireFaultLight<Engine1FaultLightData>
    {
        public Engine1FaultLight(UrsaMinorOutputs outputs) : base(outputs, 1, false) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct Engine1FireLightData : IEngineFireFaultLightData
    {
        [Property]
        [SimVar("L:I_ENG_FIRE_1", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 _isOn;
    }

    [Component, RequiredArgsConstructor]
    public partial class Engine1FireLight : EngineFireFaultLight<Engine1FireLightData>
    {
        public Engine1FireLight(UrsaMinorOutputs outputs) : base(outputs, 1, true) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct Engine2FaultLightData : IEngineFireFaultLightData
    {
        [Property]
        [SimVar("L:I_ENG_FAULT_2", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 _isOn;
    }

    [Component, RequiredArgsConstructor]
    public partial class Engine2FaultLight : EngineFireFaultLight<Engine2FaultLightData>
    {
        public Engine2FaultLight(UrsaMinorOutputs outputs) : base(outputs, 2, false) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct Engine2FireLightData : IEngineFireFaultLightData
    {
        [Property]
        [SimVar("L:I_ENG_FIRE_2", "number", SIMCONNECT_DATATYPE.INT32, 0.5f)]
        public Int32 _isOn;
    }

    [Component, RequiredArgsConstructor]
    public partial class Engine2FireLight : EngineFireFaultLight<Engine2FireLightData>
    {
        public Engine2FireLight(UrsaMinorOutputs outputs) : base(outputs, 2, true) { }
    }
}
