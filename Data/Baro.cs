﻿using System;
using System.Runtime.InteropServices;
using FSInputMapper.Event;
using Microsoft.FlightSimulator.SimConnect;

namespace FSInputMapper.Data
{

    [Singleton]
    public class KohlsmanSet : IEvent { public string SimEvent() { return "KOHLSMAN_SET"; } }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BaroData
    {
        [SCStructField("SEA LEVEL PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float seaLevelPressureMB;
        [SCStructField("KOHLSMAN SETTING MB", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB;
        [SCStructField("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanHg;
    };

    [Singleton]
    public class BaroListener : DataListener<BaroData>, IRequestDataOnOpen
    {

        private readonly DebugConsole dc;
        private readonly KohlsmanSet kohlsmanSet;

        public BaroListener(DebugConsole dc, KohlsmanSet sender)
        {
            this.dc = dc;
            this.kohlsmanSet = sender;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod()
        {
            return SIMCONNECT_PERIOD.VISUAL_FRAME;
        }

        public override void Process(SimConnect simConnect, BaroData data)
        {
            dc.Text = $"MSL {data.seaLevelPressureMB}MB"
                + $"\nKohlsman {data.kohlsmanMB}MB {data.kohlsmanHg.ToString("N2")}Hg";
            if (Math.Abs(data.seaLevelPressureMB - data.kohlsmanMB) > 0.666)
            {
                //TODO: allow the user to turn off this automatic sync.
                simConnect.SendEvent(kohlsmanSet, (uint)(data.seaLevelPressureMB * 16.0));
                dc.Text += $"\nAutomatically adjusted {DateTime.Now}";
            }
        }

    }

}
