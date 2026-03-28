using Controlzmo.GameControllers;
using Controlzmo.SimConnectzmo;
using Lombok.NET;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Controls.Engine
{
    [Component] public class EngineModeCrankEvent : IEvent { public string SimEvent() => "ENGINE_MODE_CRANK_SET"; }
    [Component] public class EngineModeNormalEvent : IEvent { public string SimEvent() => "ENGINE_MODE_NORM_SET"; }
    [Component] public class EngineModeStartEvent : IEvent { public string SimEvent() => "ENGINE_MODE_IGN_SET"; }

    [Component, RequiredArgsConstructor]
    public partial class EngineModeSender
    {
        private readonly InputEvents inputEvents;
        private readonly EngineModeNormalEvent normalEvent;

        internal void Send(ExtendedSimConnect simConnect, IEvent? standard)
        {
            if (simConnect.IsAtr7x)
            {
                double value = 0;
                if (standard is EngineModeCrankEvent)
                    value = 1;
                else if (standard is EngineModeStartEvent)
                    value = 4;
                inputEvents.Send(simConnect, "ENGINE_ENGINE_KNOB_STARTER", value);
            }
            else
                simConnect.SendEvent(standard ?? normalEvent);
        }
    }

    [Component, RequiredArgsConstructor]
    public partial class ModeCrank : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly EngineModeCrankEvent crankEvent;
        private readonly EngineModeSender sender;

        public int GetButton() => UrsaMinorThrottle.BUTTON_MODE_CRANK;

        public virtual void OnPress(ExtendedSimConnect simConnect) => sender.Send(simConnect, crankEvent);
        public virtual void OnRelease(ExtendedSimConnect simConnect) => sender.Send(simConnect, null);
    }

    [Component, RequiredArgsConstructor]
    public partial class ModeStart : IButtonCallback<UrsaMinorThrottle>
    {
        private readonly EngineModeStartEvent startEvent;
        private readonly EngineModeSender sender;

        public int GetButton() => UrsaMinorThrottle.BUTTON_MODE_START;

        public virtual void OnPress(ExtendedSimConnect simConnect) => sender.Send(simConnect, startEvent);
        public virtual void OnRelease(ExtendedSimConnect simConnect) => sender.Send(simConnect, null);
    }
}
