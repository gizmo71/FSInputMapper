using Controlzmo.Systems.Autothrust;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class TcaAirbusQuadrant : GameController
    {
        private readonly AutothrottleArmedDataListener autothrustListener;

        public TcaAirbusQuadrant(IServiceProvider sp) : base(sp, 31)
        {
            autothrustListener = sp.GetRequiredService<AutothrottleArmedDataListener>();
        }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 1031;

        public override void OnButtonChange(ExtendedSimConnect simConnect, int index, bool isPressed)
        {
            _log.LogWarning($"TCA TL button {index} = {isPressed}");
            if (isPressed)
            {
                switch (index)
                {
                    case 0: // Left intuitive disconnect
                        _log.LogDebug("User has asked to arm autothrust");
                        simConnect.RequestDataOnSimObject(autothrustListener, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
                        break;
                }
            }
        }
    }
}
