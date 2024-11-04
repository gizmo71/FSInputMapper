using Controlzmo.Systems.Autothrust;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class TcaAirbusQuadrant : GameController<TcaAirbusQuadrant>
    {
        private readonly AutothrottleArmedDataListener autothrustListener;

        public TcaAirbusQuadrant(IServiceProvider sp) : base(sp, 31, 7, 0)
        {
            autothrustListener = sp.GetRequiredService<AutothrottleArmedDataListener>();
        }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 1031;

        public static readonly int BUTTON_LEFT_INTUITIVE_DISCONNECT = 0;
        public static readonly int BUTTON_RIGHT_INTUITIVE_DISCONNECT = 1;
        public static readonly int BUTTON_LEFT_ENGINE_MASTER = 2;
        public static readonly int BUTTON_RIGHT_ENGINE_MASTER = 3;
        /* Buttons
        4 left fire "light"
        4 right fire "light"
        6 engine mode crank
        7 engine mode ignition/start
           Switches - are there any? */
        public static readonly int AXIS_LEFT_THRUST = 0;
        public static readonly int AXIS_RIGHT_THRUST = 1;
        protected override void OnUpdate(ExtendedSimConnect simConnect)
        {
            if (buttonsNew[BUTTON_LEFT_INTUITIVE_DISCONNECT] && !buttonsOld[BUTTON_LEFT_INTUITIVE_DISCONNECT])
            {
                _log.LogDebug("User has asked to arm autothrust");
                simConnect.RequestDataOnSimObject(autothrustListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
            }
        }
    }
}
