using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;

namespace Controlzmo.Systems.Lights
{
    [Component, RequiredArgsConstructor]
    public partial class WingIceLight : ISettable<bool?>
    {
        private readonly JetBridgeSender sender;

        public string GetId() => "wingIceLight";

        public void SetInSim(ExtendedSimConnect simConnect, bool? value)
        {
            var desiredValue = value == true ? 1 : 0;
            sender.Execute(simConnect, simConnect.IsFenix ?
                $"{desiredValue} (>L:S_OH_EXT_LT_WING)" :
                $"(A:LIGHT WING, Bool) {desiredValue} != if{{ 0 (>K:TOGGLE_WING_LIGHTS) }}");

// Yes, this "works"... It locks out all other camera control by default.
            if (value == true)
                simConnect.CameraAcquire("Gizmocam");
            else if (value == false)
                simConnect.CameraRelease("Gizmocam");
        }
    }

    [Component]
    public partial class TestDaOutputs : IAxisCallback<T16000mHotas>
    {
        public int GetAxis() => T16000mHotas.AXIS_WHEEL;

        public void OnChange(ExtendedSimConnect simConnect, double old, double @new)
        {
Console.WriteLine($"**--** @new {@new - 0.5}");
            SIMCONNECT_DATA_CAMERA data = new SIMCONNECT_DATA_CAMERA {
                PositionReferential = SIMCONNECT_POSITION_REFERENTIAL.EYEPOINT,
                // x is left/right, with + being left(!)
                // y is up/down, with + being up
                // z is fore/aft with + being fore
                // Note that the eyepoint in the Fenix is actually where the pilot's bum sits, and in the A330 it's in the cabin!
                Position = new SIMCONNECT_DATA_XYZ { x = 0.0, y = 0.0, z = 15.0 * (@new - 0.5) },
            };
            SIMCONNECT_CAMERA_DATA_MASK mask = SIMCONNECT_CAMERA_DATA_MASK.POSITION;
            simConnect.CameraSet(data, (uint)mask);
        }
    }
}
