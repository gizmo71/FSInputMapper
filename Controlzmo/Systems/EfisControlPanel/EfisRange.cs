using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.EfisControlPanel
{
    public interface IEfisRangeData
    {
        public UInt32 RangeCode { get; set; } // 2^code*10 = miles
        public UInt32 RangeFenix { get; set; } // 0 for 10 to 5 for 320 (same as FBW)
    }

    public abstract class EfisRange<T> : DataListener<T>, ISettable<string>, IRequestDataOnOpen where T : struct, IEfisRangeData
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly string id;

        protected EfisRange(IServiceProvider serviceProvider, string side)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            id = $"{side}EfisNdRange";
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, T data)
        {
            hub.Clients.All.SetFromSim(id, (1 << (int) data.RangeCode) * 10);
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            var range = UInt32.Parse(label!);
            var code = (UInt32) Math.Clamp(BitOperations.Log2(range / 10), 0, 5);
            simConnect.SendDataOnSimObject(new T() { RangeCode = code, RangeFenix = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisRangeData : IEfisRangeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_ND_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _rangeCode;
        [Property]
        [SimVar("L:S_FCU_EFIS1_ND_ZOOM", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _rangeFenix;
    };

    [Component]
    public class LeftEfisRange : EfisRange<LeftEfisRangeData>
    {
        public LeftEfisRange(IServiceProvider serviceProvider) : base(serviceProvider, "left") { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct RightEfisRangeData : IEfisRangeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_R_ND_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _rangeCode;
        [Property]
        [SimVar("L:S_FCU_EFIS2_ND_ZOOM", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _rangeFenix;
    };

    //[Component]
    public class RightEfisRange : EfisRange<RightEfisRangeData>
    {
        public RightEfisRange(IServiceProvider serviceProvider) : base(serviceProvider, "right") { }
    }
}
