using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerData
    {
        [SCStructField("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersHandlePosition;
        [SCStructField("SPOILERS ARMED", "Bool", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersArmed;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpoilerHandle
    {
        [SCStructField("SPOILERS HANDLE POSITION", "percent", SIMCONNECT_DATATYPE.INT32, 0f)]
        public Int32 spoilersHandlePosition;
    };

    [Singleton]
    public class SpoilerHandleSender : DataSender<SpoilerHandle>
    {
        public SpoilerHandleSender(SimConnectHolder sch) : base(sch) { }
    }

    [Singleton]
    public class MoreSpoilerListener : DataListener<SpoilerData>
    {
        private readonly SpoilerHandleSender sender;

        public MoreSpoilerListener(SpoilerHandleSender sender) { this.sender = sender; }

        public override void Process(SimConnect simConnect, SpoilerData spoilerData)
        {
            if (spoilerData.spoilersArmed != 0)
                simConnect.SendEvent(EVENT.DISARM_SPOILER);
            else if (spoilerData.spoilersHandlePosition < 100)
                sender.Send(new SpoilerHandle { spoilersHandlePosition = Math.Min(spoilerData.spoilersHandlePosition + 25, 100) });
        }
    }

    [Singleton]
    public class LessSpoilerListener : DataListener<SpoilerData>
    {
        private readonly SpoilerHandleSender sender;

        public LessSpoilerListener(SpoilerHandleSender sender) { this.sender = sender; }

        public override void Process(SimConnect simConnect, SpoilerData spoilerData)
        {
            if (spoilerData.spoilersHandlePosition > 0)
                sender.Send(new SpoilerHandle { spoilersHandlePosition = Math.Max(spoilerData.spoilersHandlePosition - 25, 0) });
            else if (spoilerData.spoilersArmed == 0)
                simConnect.SendEvent(EVENT.ARM_SPOILER);
        }
    }

}
