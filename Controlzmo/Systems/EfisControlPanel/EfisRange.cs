using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
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
        public Int32 RangeCode { get; set; } // Generic/old A32NX; A380X: values are A32NX+1, and 0 means use OANS range instead
        public Int32 RangeA32nx {  get; set; } // A32NX: 2^code*10 = miles
        public Int32 OansRange { get; set; } // In Zoom, this goes from 0 (most zoomed in) to 4 (least, which is just "under" range 10)
        public Int32 RangeFenix { get; set; } // 0 for 10 to 5 for 320 (same as A32NX)
        public Int32 RangeIni { get; set; } // (same as A32NX and Fenix)
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

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.VISUAL_FRAME;

        public override void Process(ExtendedSimConnect simConnect, T data)
        {
            if (simConnect.IsFenix) data.RangeCode = data.RangeFenix;
            else if (simConnect.IsA32NX) data.RangeCode = data.RangeA32nx;
            else if (simConnect.IsIniBuilds) data.RangeCode = data.RangeIni;
            int value = data.RangeCode;
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
            // There's no Zoom or 640 range in the A320 family:
            else code = Math.Clamp(code - 1, 0, 5);
            simConnect.SendDataOnSimObject(new T() { RangeCode = code, RangeA32nx = code, OansRange = oans, RangeFenix = code, RangeIni = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisRangeData : IEfisRangeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_ND_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 _rangeCode;
        [Property]
        [SimVar("L:A32NX_FCU_EFIS_L_EFIS_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 _rangeA32nx;
        [Property]
        [SimVar("L:A32NX_EFIS_L_OANS_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 _oansRange;
        [Property]
        [SimVar("L:S_FCU_EFIS1_ND_ZOOM", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 _rangeFenix;
        [Property]
        [SimVar("L:INI_MAP_RANGE_CAPT_SWITCH", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public Int32 _rangeIni;
    };

    [Component]
    public class LeftEfisRange : EfisRange<LeftEfisRangeData>
    {
        public LeftEfisRange(IServiceProvider serviceProvider) : base(serviceProvider, "left") { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct A380xEfisRangeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_ND_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 standard;
        [Property]
        [SimVar("L:A32NX_EFIS_L_OANS_RANGE", "number", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 oans;
    };

    [Component, RequiredArgsConstructor]
    public partial class EfisStickRange : DataListener<A380xEfisRangeData>, IAxisCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        private int delta;

        public int GetAxis() => UrsaMinorFighterR.AXIS_MINI_STICK_Y;

        public void OnChange(ExtendedSimConnect simConnect, double old, double @new)
        {
            if (old >= 0.25 && @new < 0.25) Move(simConnect, "--");
            else if (old <= 0.75 && @new > 0.75) Move(simConnect,"++");
        }
        public override void Process(ExtendedSimConnect simConnect, A380xEfisRangeData data)
        {
            int old = data.standard == 0 ? data.oans : data.standard + 4;
            int @new = Math.Max(Math.Min(old + delta, 11), 0);
            data.standard = Math.Max(0, @new - 4);
            data.oans = Math.Min(4, @new);
            simConnect.SendDataOnSimObject(data);
        }

        private void Move(ExtendedSimConnect simConnect, string op)
        {
            var lvar = "A32NX_EFIS_L_ND_RANGE";
            var min = 0;
            var max = 5; //TODO: does the A330 support 6 like the A380X does?
            if (simConnect.IsA380X)
            {
                delta = op == "++" ? 1 : -1;
                simConnect.RequestDataOnSimObject(this, SIMCONNECT_CLIENT_DATA_PERIOD.ONCE);
                return;
            }
            else if (simConnect.IsA32NX || simConnect.IsA339) lvar = "A32NX_FCU_EFIS_L_EFIS_RANGE";
            else if (simConnect.IsFenix) lvar = "S_FCU_EFIS1_ND_ZOOM";
            else if (simConnect.IsIniBuilds) lvar = "INI_MAP_RANGE_CAPT_SWITCH";
            sender.Execute(simConnect, $"(L:{lvar}) {op} {min} max {max} min (>L:{lvar})");
        }
    }
}
