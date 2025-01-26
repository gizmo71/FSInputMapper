using Controlzmo.GameControllers;
using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
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
        public UInt32 ModeA32nx { get; set; }
        public UInt32 ModeFenix { get; set; }
        public UInt32 ModeIni { get; set; }
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
            [5u] = "Eng", // A330 only!
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
            if (simConnect.IsFenix) data.Mode = data.ModeFenix;
            if (simConnect.IsA32NX) data.Mode = data.ModeA32nx;
            else if (simConnect.IsIniBuilds) data.Mode = data.ModeIni;
            hub.Clients.All.SetFromSim(id, ModeMap[data.Mode]);
        }

        public string GetId() => id;

        public void SetInSim(ExtendedSimConnect simConnect, string? label)
        {
            if (label == "Eng" && !simConnect.IsA330)
                return;
            var value = ModeMap.Inverse[label!];
            simConnect.SendDataOnSimObject(new T() { Mode = value, ModeA32nx = value, ModeFenix = value, ModeIni = value });
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public partial struct LeftEfisModeData : IEfisModeData
    {
        [Property]
        [SimVar("L:A32NX_EFIS_L_ND_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _mode;
        [Property]
        [SimVar("L:A32NX_FCU_EFIS_L_EFIS_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _modeA32nx;
        [Property]
        [SimVar("L:S_FCU_EFIS1_ND_MODE", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _modeFenix;
        [Property]
        [SimVar("L:INI_MAP_MODE_CAPT_SWITCH", "number", SIMCONNECT_DATATYPE.INT32, 0.4f)]
        public UInt32 _modeIni;
    };

    [Component]
    public class EfisLeftMode : EfisMode<LeftEfisModeData>
    {
        public EfisLeftMode(IServiceProvider sp) : base(sp, "left") { }
    }

    [Component, RequiredArgsConstructor]
    public partial class EfisStickMode : IAxisCallback<UrsaMinorFighterR>
    {
        private readonly JetBridgeSender sender;

        public int GetAxis() => UrsaMinorFighterR.AXIS_MINI_STICK_X;

        public void OnChange(ExtendedSimConnect simConnect, double old, double @new)
        {
            if (old >= 0.25 && @new < 0.25) Move(simConnect, "--");
            else if (old <= 0.75 && @new > 0.75) Move(simConnect,"++");
        }

        private void Move(ExtendedSimConnect simConnect, string op)
        {
            var lvar = "A32NX_EFIS_L_ND_MODE";
            if (simConnect.IsFenix) lvar = "S_FCU_EFIS1_ND_MODE";
            if (simConnect.IsA32NX || simConnect.IsA339) lvar = "A32NX_FCU_EFIS_L_EFIS_MODE";
            else if (simConnect.IsIniBuilds) lvar = "INI_MAP_MODE_CAPT_SWITCH";
            var min = 0;
            var max = simConnect.IsA330 ? 5 : 4;
            sender.Execute(simConnect, $"(L:{lvar}) {op} {min} max {max} min (>L:{lvar})");
        }
    }
}
