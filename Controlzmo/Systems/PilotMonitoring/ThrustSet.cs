using System;
using System.Runtime.InteropServices;
using Controlzmo.Hubs;
using Controlzmo.SimConnectzmo;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.PilotMonitoring
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ThrustSetData
    {
        [SimVar("ENG N1 RPM:1", "Number", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 engine1N1;
        [SimVar("ENG N1 RPM:2", "Number", SIMCONNECT_DATATYPE.INT32, 1.0f)]
        public Int32 engine1N2;
    };

    [Component]
    public class ThrustLimit : LVar
    {
        public ThrustLimit(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOTHRUST_THRUST_LIMIT";
        protected override int Milliseconds() => 1000;
        protected override double Default() => -1.0;
    }

    [Component]
    public class Commanded1N1 : LVar
    {
        public Commanded1N1(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOTHRUST_N1_COMMANDED:1";
        protected override int Milliseconds() => 1000;
        protected override double Default() => -1.0;
    }

    [Component]
    public class ThrustLever1N1 : LVar
    {
        public ThrustLever1N1(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOTHRUST_TLA_N1:1";
        protected override int Milliseconds() => 1000;
        protected override double Default() => -1.0;
    }

    [Component] // 1 (TOGA) or 3 (FLEX)
    public class AutothrustMode1 : LVar
    {
        public AutothrustMode1(IServiceProvider serviceProvider) : base(serviceProvider) { }
        protected override string LVarName() => "A32NX_AUTOTHRUST_MODE:1";
        protected override int Milliseconds() => 1000;
        protected override double Default() => -1.0;
    }

    [Component]
    public class ThrustListener : DataListener<ThrustSetData>
    {
        private readonly AutothrustMode1 mode1;
        private readonly ThrustLever1N1 lever1;
        private readonly Commanded1N1 commanded1;
        private readonly ThrustLimit limit;
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hubContext;

        public ThrustListener(IServiceProvider serviceProvider)
        {
            mode1 = serviceProvider.GetRequiredService<AutothrustMode1>();
            lever1 = serviceProvider.GetRequiredService<ThrustLever1N1>();
            commanded1 = serviceProvider.GetRequiredService<Commanded1N1>();
            limit = serviceProvider.GetRequiredService<ThrustLimit>();
            hubContext = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            serviceProvider.GetRequiredService<RunwayCallsStateListener>().onGroundHandlers += OnGroundHandler;
        }

        private void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            SIMCONNECT_PERIOD period = isOnGround ? SIMCONNECT_PERIOD.SECOND : SIMCONNECT_PERIOD.NEVER;
            simConnect.RequestDataOnSimObject(this, period);
hubContext.Clients.All.Speak((isOnGround ? "" : "not ") + "on ground in Thrust Set listener");
        }

        public override void Process(ExtendedSimConnect simConnect, ThrustSetData data)
        {
hubContext.Clients.All.Speak("Thrust Set listener got data");
            // Currently the thrust limits are hard coded in the mod to 81% for FLEX and 85% for TOGA.
            System.Console.Error.WriteLine($"Thrust {data.engine1N1}/{data.engine1N2} m1 {(double?)mode1} l1 {(double?)lever1} c1 {(double?)commanded1} lim {(double?)limit}");
        }
    }
}
