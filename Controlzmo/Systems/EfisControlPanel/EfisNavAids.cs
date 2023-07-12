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
    public interface IEfisNavAidData
    {
        public UInt32 Mode { get; set; }
    }

    public abstract class EfisNavAid<T> : DataListener<T>, ISettable<string>, IRequestDataOnOpen where T : struct, IEfisNavAidData
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        protected readonly string id;

        protected EfisNavAid(IServiceProvider serviceProvider, string side, int number)
        {
            hub = serviceProvider.GetRequiredService<IHubContext<ControlzmoHub, IControlzmoHub>>();
            id = $"{side}EfisNavAid{number}";
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, T data)
        {
            hub.Clients.All.SetFromSim(id, data.Mode switch
            {
                0u => "Off",
                1u => "ADF",
                2u => "VOR",
                _ => throw new ArgumentOutOfRangeException($"Unrecognised EFIS navaid type '{data.Mode}'")
            });
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            var code = label switch
            {
                "Off" => 0u,
                "ADF" => 1u,
                "VOR" => 2u,
                _ => throw new ArgumentOutOfRangeException($"Unrecognised EFIS navaid setting '{label}'")
            };
            simConnect.SendDataOnSimObject(new T() { Mode = code });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisNavAid1Data : IEfisNavAidData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_NAVAID_1_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _mode;
    };

    [Component]
    public class EfisLeftNavAid1 : EfisNavAid<LeftEfisNavAid1Data>
    {
        public EfisLeftNavAid1(IServiceProvider sp) : base(sp, "left", 1) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisNavAid2Data : IEfisNavAidData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_NAVAID_2_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _mode;
    };

    [Component]
    public class EfisLeftNavAid2 : EfisNavAid<LeftEfisNavAid2Data>
    {
        public EfisLeftNavAid2(IServiceProvider sp) : base(sp, "left", 2) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct RightEfisNavAid1Data : IEfisNavAidData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_R_NAVAID_1_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _mode;
    };

    //[Component]
    public class EfisRightNavAid1 : EfisNavAid<RightEfisNavAid1Data>
    {
        public EfisRightNavAid1(IServiceProvider sp) : base(sp, "right", 1) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct RightEfisNavAid2Data : IEfisNavAidData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_R_NAVAID_2_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _mode;
    };

    //[Component]
    public class EfisRightNavAid2 : EfisNavAid<RightEfisNavAid2Data>
    {
        public EfisRightNavAid2(IServiceProvider sp) : base(sp, "right", 2) { }
    }
}
