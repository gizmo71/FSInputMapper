using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.Controls
{
    [Component] public class SetAileronsEvent : IEvent { public string SimEvent() => "AXIS_AILERONS_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class Ailerons : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly SetAileronsEvent _event;
        private readonly AxisSetter setter;
        public int GetAxis() => UrsaMinorFighterR.AXIS_ROLL;
        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.SetAvoidingCentreRepetition(sc, _event, old, @new, 0.505);
    }

    [Component, RequiredArgsConstructor]
    public partial class AileronsTm : IAxisCallback<TmSkyYoke>
    {
        private readonly Ailerons stick;
        public int GetAxis() => TmSkyYoke.AXIS_ROLL;
        public void OnChange(ExtendedSimConnect sc, double old, double @new) => stick.OnChange(sc, old, @new);
    }

    [Component] public class SetElevatorsEvent : IEvent { public string SimEvent() => "AXIS_ELEVATOR_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class Elevators : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly SetElevatorsEvent _event;
        private readonly AxisSetter setter;
        public int GetAxis() => UrsaMinorFighterR.AXIS_PITCH;
        public void OnChange(ExtendedSimConnect sc, double old, double @new) => setter.SetAvoidingCentreRepetition(sc, _event, old, @new, 0.5);
    }

    [Component, RequiredArgsConstructor]
    public partial class ElevatorsTm : IAxisCallback<TmSkyYoke>
    {
        private readonly Elevators stick;
        public int GetAxis() => TmSkyYoke.AXIS_PITCH;
        public void OnChange(ExtendedSimConnect sc, double old, double @new) => stick.OnChange(sc, old, @new);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct YokeXData
    {
        [SimVar("YOKE X POSITION", "position 16k", SIMCONNECT_DATATYPE.INT32, 16f)]
        public Int32 xPos;
        [SimVar("YOKE X POSITION WITH AP", "position 16k", SIMCONNECT_DATATYPE.INT32, 16f)]
        public Int32 xPosAp;
    }

    [Component, RequiredArgsConstructor]
    public partial class YokeXPos : DataListener<YokeXData>//, IRequestDataOnOpen
    {
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, YokeXData data)
        {
            Console.WriteLine($"Yoke X with AP {data.xPosAp} without {data.xPos}");
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct YokeYData
    {
        [SimVar("YOKE Y POSITION", "position 16k", SIMCONNECT_DATATYPE.INT32, 16f)]
        public Int32 yPos;
        [SimVar("YOKE Y POSITION WITH AP", "position 16k", SIMCONNECT_DATATYPE.INT32, 16f)]
        public Int32 yPosAp;
    }

    [Component, RequiredArgsConstructor]
    public partial class YokeYPos : DataListener<YokeYData>//, IRequestDataOnOpen
    {
        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, YokeYData data)
        {
            Console.WriteLine($"Yoke Y with AP {data.yPosAp} without {data.yPos}");
        }
    }
}
