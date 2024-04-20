using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.EfisControlPanel
{
    public interface IEfisModeData
    {
        public UInt32 Mode { get; set; }
        public UInt32 ModeFenix { get; set; }
    }

    public abstract class EfisMode<T> : DataListener<T>, ISettable<string>, IRequestDataOnOpen where T : struct, IEfisModeData
    {
        private readonly BidirectionalDictionary<UInt32, string> ModeMap = new()
        {
            [0u] = "Rose ILS",
            [1u] = "Rose VOR",
            [2u] = "Rose Nav",
            [3u] = "Arc",
            [4u] = "Plan",
        };
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        protected readonly string id;

        protected EfisMode(IServiceProvider serviceProvider, string side)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            id = $"{side}EfisNdMode";
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, T data)
        {
            hub.Clients.All.SetFromSim(id, ModeMap[data.Mode]);
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            var value = ModeMap.Inverse[label!];
            simConnect.SendDataOnSimObject(new T() { Mode = value, ModeFenix = value });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisModeData : IEfisModeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_ND_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _mode;
        [Property]
        [SimVar("L:S_FCU_EFIS1_ND_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _modeFenix;
    };

    [Component]
    public class EfisLeftMode : EfisMode<LeftEfisModeData>
    {
        public EfisLeftMode(IServiceProvider sp) : base(sp, "left") { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct RightEfisModeData : IEfisModeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_ND_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _mode;
        [Property]
        [SimVar("L:S_FCU_EFIS2_ND_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _modeFenix;
    };

    //[Component]
    public class EfisRightMode : EfisMode<RightEfisModeData>
    {
        public EfisRightMode(IServiceProvider sp) : base(sp, "right") { }
    }
}
