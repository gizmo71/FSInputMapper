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

        /* Buttons
        0 left intuitive disconnect
        1 rigth intuitive disconnect
        2 left engine master
        3 right engine master
        4 left fire "light"
        4 right fire "light"
        6 engine mode crank
        7 engine mode ignition/start
        /* Axes
        0 left thrust lever (0 TOGA->1 max reverse)
        1 right thrust lever (0 TOGA->1 max reverse)
        /* Switches
         */
        private static readonly int BUTTON_LEFT_INTUITIVE_DISCONNECT = 0;
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
