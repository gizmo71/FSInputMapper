using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TakeOffData
    {
        [SimVar("AIRSPEED INDICATED", "Knots", SIMCONNECT_DATATYPE.INT32, 0.99f)]
        public Int32 kias;
    };

    [Component]
    public class TakeOffListener : DataListener<TakeOffData>
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;
        private readonly LocalVarsListener localVarsListener;

        bool wasAbove80 = false;
        bool wasAboveV1 = false;
        bool wasAboveVR = false;

        public TakeOffListener(IServiceProvider serviceProvider)
        {
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            localVarsListener = serviceProvider.GetRequiredService<LocalVarsListener>();
        }

        public override void Process(ExtendedSimConnect simConnect, TakeOffData data)
        {
            System.Console.Error.WriteLine($"Takeoff: KIAS {data.kias}");
            setAndCallIfRequired(80, 66, "eighty knots", ref wasAbove80);
            setAndCallIfRequired(localVarsListener.localVars.v1, 66, "vee one", ref wasAboveV1);
            setAndCallIfRequired(localVarsListener.localVars.vr, 66, "rotate", ref wasAboveVR);
        }

        private void setAndCallIfRequired(double calledSpeed, double actualSpeed, string call, ref bool wasAbove)
        {
            bool isAbove = calledSpeed > 0 && actualSpeed >= calledSpeed;
            if (isAbove && !wasAbove)
            {
                hubContext.Clients.All.Speak(call);
            }
            wasAbove = isAbove;
        }
    }
}
