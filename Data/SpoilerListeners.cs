using System;
using System.Runtime.InteropServices;
using FSInputMapper.Event;
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
    public class MoreSpoilerListener : DataListener<SpoilerData>
    {

        private readonly LessSpoilerEvent toggleArmSpoiler;

        public MoreSpoilerListener(LessSpoilerEvent toggleArmSpoiler)
        {
            this.toggleArmSpoiler = toggleArmSpoiler;
        }

        public override void Process(SimConnect simConnect, SpoilerData spoilerData)
        {
            if (spoilerData.spoilersArmed != 0)
                simConnect.SendEvent(toggleArmSpoiler);
            else if (spoilerData.spoilersHandlePosition < 100)
                simConnect.SetDataOnSimObject(new SpoilerHandle {
                    spoilersHandlePosition = Math.Min(spoilerData.spoilersHandlePosition + 25, 100) });
        }
    }

    [Singleton]
    public class LessSpoilerListener : DataListener<SpoilerData>
    {

        private readonly LessSpoilerEvent toggleArmSpoiler;

        public LessSpoilerListener(LessSpoilerEvent toggleArmSpoiler)
        {
            this.toggleArmSpoiler = toggleArmSpoiler;
        }

        public override void Process(SimConnect simConnect, SpoilerData spoilerData)
        {
            if (spoilerData.spoilersHandlePosition > 0)
                simConnect.SetDataOnSimObject(new SpoilerHandle {
                    spoilersHandlePosition = Math.Max(spoilerData.spoilersHandlePosition - 25, 0) });
            else if (spoilerData.spoilersArmed == 0)
                simConnect.SendEvent(toggleArmSpoiler);
        }

    }

}
