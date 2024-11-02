using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.EfisControlPanel
{
    public interface IEfisRangeData
    {
        public Int32 RangeCode { get; set; } // A32NX: 2^code*10 = miles; A380X: values are A32NX+1, and 0 means use OANS range instead
        public Int32 OansRange { get; set; } // In Zoom, this goes from 0 (most zoomed in) to 4 (least, which is just "under" range 10)
        public Int32 RangeFenix { get; set; } // 0 for 10 to 5 for 320 (same as A32NX)
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
            int value = simConnect.IsFenix ? data.RangeFenix : data.RangeCode;
            if (simConnect.IsA380X) { if (value == 0) value = data.OansRange - 4; }
            else ++value;
            hub.Clients.All.SetFromSim(id, value);
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            var code = Math.Clamp(Int32.Parse(label!), -4, 7);
            var oans = 4;
            if (simConnect.IsA380X) { if (code < 0) { oans = code + 4; code = 0; } }
            else if (code < 0 || code > 5)
                return; // There's no Zoom or 640 range in the A320 family
            simConnect.SendDataOnSimObject(new T() { RangeCode = code, OansRange = oans, RangeFenix = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisRangeData : IEfisRangeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_ND_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 _rangeCode;
        [Property]
        [SimVar("L:A32NX_EFIS_L_OANS_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 _oansRange;
        [Property]
        [SimVar("L:S_FCU_EFIS1_ND_ZOOM", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 _rangeFenix;
    };

    [Component]
    public class LeftEfisRange : EfisRange<LeftEfisRangeData>
    {
        public LeftEfisRange(IServiceProvider serviceProvider) : base(serviceProvider, "left") { }
    }
}
