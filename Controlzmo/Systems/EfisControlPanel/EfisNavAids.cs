﻿using Controlzmo.Hubs;
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
    public interface IEfisNavAidData
    {
        public UInt32 Mode { get; set; }
        public UInt32 ModeFenix { get; set; }
    }

    public abstract class EfisNavAid<T> : DataListener<T>, ISettable<string>, IRequestDataOnOpen where T : struct, IEfisNavAidData
    {
        private readonly BidirectionalDictionary<UInt32, string> ModeMap = new()
        {
            [0u] = "Off",
            [1u] = "ADF",
            [2u] = "VOR",
        };
        private readonly IDictionary<string, UInt32> MapModeFenix = new Dictionary<string, UInt32>()
        { // Can't read values from sim. :-(
            ["ADF"] = 0u,
            ["Off"] = 1u,
            ["VOR"] = 2u,
        };
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
            if (simConnect.IsFBW) {
                var value = ModeMap[data.Mode];
                hub.Clients.All.SetFromSim(id, value);
            }
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            var value = (simConnect.IsFenix ? MapModeFenix : ModeMap.Inverse)[label!];
            simConnect.SendDataOnSimObject(new T() { Mode = value, ModeFenix = value });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisNavAid1Data : IEfisNavAidData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_NAVAID_1_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _mode;
        [Property]
        [SimVar("L:S_FCU_EFIS1_NAV1", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _modeFenix;
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
        [Property]
        [SimVar("L:S_FCU_EFIS1_NAV2", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _modeFenix;
    };

    [Component]
    public class EfisLeftNavAid2 : EfisNavAid<LeftEfisNavAid2Data>
    {
        public EfisLeftNavAid2(IServiceProvider sp) : base(sp, "left", 2) { }
    }
}
