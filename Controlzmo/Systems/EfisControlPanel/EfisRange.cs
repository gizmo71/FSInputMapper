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
        public UInt32 RangeCode { get; set; } // A32NX: 2^code*10 = miles; A380X uses 0 for Zoom, so all the others are shifted up one
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
            var value = (1 << (int) (simConnect.IsFenix ? data.RangeFenix : data.RangeCode)) * 10;
            if (simConnect.IsA380X) value /= 2;
            hub.Clients.All.SetFromSim(id, value);
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            var range = UInt32.Parse(label!);
            var code = (UInt32) Math.Clamp(BitOperations.Log2(range / 10), -1, 6);
            if (simConnect.IsA380X)
                ++code;
            else if (code < 0 || code > 5)
                return; // There's no 640 range in the A320 family
            simConnect.SendDataOnSimObject(new T() { RangeCode = code, RangeFenix = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisRangeData : IEfisRangeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_ND_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _rangeCode; // In the A380X, 1 is 10 and 7 is 640, but there is also 0 for "ZOOM" which isn't user selectable on the knob...
        [Property]
        [SimVar("L:S_FCU_EFIS1_ND_ZOOM", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _rangeFenix;
    };

    [Component]
    public class LeftEfisRange : EfisRange<LeftEfisRangeData>
    {
        public LeftEfisRange(IServiceProvider serviceProvider) : base(serviceProvider, "left") { }
    }
}
