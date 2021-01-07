using System.Runtime.InteropServices;
using System;
using Controlzmo;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;

namespace Controlzmo.Systems.Spoilers
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerData
    {
        [SimVar("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersHandlePosition;
        [SimVar("SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersArmed;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerHandle : IData<SpoilerHandle>
    {
        [SimVar("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersHandlePosition;
    };

    [Component]
    public class SpoilerHandleSender : DataSender<SpoilerHandle> { }

    [Component]
    public class MoreSpoilerListener : DataListener<SpoilerData>
    {
        private readonly LessSpoilerEvent toggleArmSpoiler;
        private readonly SpoilerHandleSender spoilerHandleSender;

        public MoreSpoilerListener(LessSpoilerEvent toggleArmSpoiler, SpoilerHandleSender spoilerHandleSender)
        {
            this.toggleArmSpoiler = toggleArmSpoiler;
            this.spoilerHandleSender = spoilerHandleSender;
        }

        public override void Process(ExtendedSimConnect simConnect, SpoilerData spoilerData)
        {
            SpoilerHandle newPosition = new();
            if (spoilerData.spoilersArmed != 0)
                simConnect.SendEvent(toggleArmSpoiler);
            else if (spoilerData.spoilersHandlePosition < 100)
                newPosition.spoilersHandlePosition = Math.Min(spoilerData.spoilersHandlePosition + 25, 100);
            spoilerHandleSender.Send(simConnect, newPosition);
        }
    }

    [Component]
    public class LessSpoilerListener : DataListener<SpoilerData>
    {
        private readonly LessSpoilerEvent toggleArmSpoiler;
        private readonly SpoilerHandleSender spoilerHandleSender;

        public LessSpoilerListener(LessSpoilerEvent toggleArmSpoiler, SpoilerHandleSender spoilerHandleSender)
        {
            this.toggleArmSpoiler = toggleArmSpoiler;
            this.spoilerHandleSender = spoilerHandleSender;
        }

        public override void Process(ExtendedSimConnect simConnect, SpoilerData spoilerData)
        {
            if (spoilerData.spoilersHandlePosition > 0)
                spoilerHandleSender.Send(simConnect, new SpoilerHandle
                {
                    spoilersHandlePosition = Math.Max(spoilerData.spoilersHandlePosition - 25, 0)
                });
            else if (spoilerData.spoilersArmed == 0)
                simConnect.SendEvent(toggleArmSpoiler);
        }
    }

    [Component]
    public class MoreSpoilerEvent : IEvent
    {
        public string SimEvent() { return "SPOILERS_TOGGLE"; }
    }

    [Component]
    public class LessSpoilerEvent : IEvent
    {
        public string SimEvent() { return "SPOILERS_ARM_TOGGLE"; }
    }

    public abstract class SpoilerEventNotification : IEventNotification
    {

        private readonly IEvent trigger;
        private readonly DataListener<SpoilerData> listener;

        protected SpoilerEventNotification(DataListener<SpoilerData> listener, IEvent trigger)
        {
            this.listener = listener;
            this.trigger = trigger;
        }

        public IEvent GetEvent() { return trigger; }

        public void OnRecieve(ExtendedSimConnect simConnect, SIMCONNECT_RECV_EVENT data)
        {
            simConnect.RequestDataOnSimObject(listener, SIMCONNECT_PERIOD.ONCE);
        }

    }

    [Component]
    public class LessSpoilerEventNotification : SpoilerEventNotification
    {
        public LessSpoilerEventNotification(LessSpoilerListener listener, LessSpoilerEvent e) : base(listener, e) { }
    }

    [Component]
    public class MoreSpoilerEventNotification : SpoilerEventNotification
    {
        public MoreSpoilerEventNotification(MoreSpoilerListener listener, MoreSpoilerEvent e) : base(listener, e) { }
    }
}
