using Controlzmo.GameControllers;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Views
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DroneZoomData
    {
        [SimVar("DRONE CAMERA FOV", "percent", SIMCONNECT_DATATYPE.INT32, 0.0f)]
        public Int32 fovPercentage; // 50% is "neutral"
    }

    [Component]
    [RequiredArgsConstructor]
    public partial class DroneZoom : DataListener<DroneZoomData>
    {
        private readonly CameraState cameraState;

        private Int32 delta = 0;
        private DateTime stoppedUntil = DateTime.Now;

        internal void Change(ExtendedSimConnect simConnect, Int32 delta)
        {
            this.delta = delta;
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.VISUAL_FRAME, true);
        }

        public override void Process(ExtendedSimConnect simConnect, DroneZoomData data)
        {
            Int32 newValue = Math.Max(Math.Min(data.fovPercentage + delta, 100), 0);

            if (cameraState.Current != CameraState.DRONE || delta == 0 || newValue == data.fovPercentage) {
//Console.Error.WriteLine($"Nowt, because: {cameraState.Current} !CS= {CameraState.DRONE}, or !{delta}, or {newValue} == {data.fovPercentage}");
                simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.NEVER);
            }
            else if (DateTime.Now >= stoppedUntil)
            {
                if (newValue >= 50 && data.fovPercentage < 50 || newValue <= 50 && data.fovPercentage > 50)
                {
                    newValue = 50;
                    stoppedUntil = DateTime.Now.AddMilliseconds(750);
                }
                else if (newValue == 29 && data.fovPercentage < 29)
                    newValue = 30; // Some wierd problem going up only to 29...
                else if (newValue == 58 && data.fovPercentage < 58)
                    newValue = 59; // ... and 59 in 2024!
//Console.Error.WriteLine($"Setting drone zoom from {data.fovPercentage} to {newValue} with delta {delta}");
                data.fovPercentage = newValue;
                simConnect.SendDataOnSimObject(data);
            }
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class ExperimentalEventListener : IAxisCallback<T16000mHotas>
    {
        private readonly DroneZoom droneZoom;

        public int GetAxis() => T16000mHotas.AXIS_RUDDER_PADDLES;

        public void OnChange(ExtendedSimConnect sc, double old, double @new)
        {
            var delta = 0;
            if (@new < 0.49) delta = -(int)Math.Ceiling((0.49 - @new) / 0.49 * 5);
            else if (@new > 0.51) delta = (int)Math.Ceiling((@new - 0.51) / 0.51 * 5);
//Console.Error.WriteLine($"new drone zoom raw {@new}");
            droneZoom.Change(sc, delta);
        }
    }
}
