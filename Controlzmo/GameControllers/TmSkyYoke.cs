using Microsoft.Extensions.Logging;
using SimConnectzmo;
using System;

namespace Controlzmo.GameControllers
{
    [Component]
    public class TmSkyYoke : GameController<TmSkyYoke>
    {
        public TmSkyYoke(IServiceProvider sp) : base(sp, 24+10, 8+10, 1+10) { }

        public override ushort Vendor() => 1103;
        public override ushort Product() => 10245;

        public static readonly int BUTTON_BLA = 0;

        /// <summary>0 left->1 right (with bottom light off)</summary>
        public static readonly int AXIS_BLA = 0;

        /// <summary>0 left->1 right (with bottom light on)</summary>
        public static readonly int SWITCH_BLA = 0;

        protected override void OnUpdate(ExtendedSimConnect simConnect) {
            for (int i = 0; i < axesOld.Length; ++i)
                if (axesOld[i] != axesNew[i])
                    _log.LogDebug($"SkyYoke: axes[{i}] {axesOld[i]} -> {axesNew[i]}");
            for (int i = 0; i < this.buttonsOld.Length; ++i)
                if (buttonsOld[i] != buttonsNew[i])
                    _log.LogDebug($"SkyYoke: button {i} now {buttonsNew[i]}");
            for (int i = 0; i < this.switchesOld.Length; ++i)
                if (switchesOld[i] != switchesNew[i])
                    _log.LogDebug($"SkyYoke: switch {i} now {switchesNew[i]}");
        }
    }
}
