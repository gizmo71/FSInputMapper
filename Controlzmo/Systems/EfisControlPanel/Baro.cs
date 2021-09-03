using Controlzmo.Hubs;
using Controlzmo.Serial;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

namespace Controlzmo.Systems.EfisControlPanel
{
    [Component]
    public class BaroKnob : ISettable<string>
    {
        private readonly JetBridgeSender sender;

        public BaroKnob(JetBridgeSender sender) => this.sender = sender;

        public string GetId() => "baroKnob";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            string command;
            if (value == "pull")
                command = @"
(L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_BARO# == if{
    #MODE_STD_BARO# (>L:XMLVAR_Baro#BARO_ID#_Mode)
} els{
    (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_QNH# == if{
        #MODE_STD_QNH# (>L:XMLVAR_Baro#BARO_ID#_Mode)
    }
}";
            else if (value == "push")
                command = @"
(L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_BARO# == if{
    #MODE_BARO# (>L:XMLVAR_Baro#BARO_ID#_Mode)
} els{
    (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_QNH# == if{
        #MODE_QNH# (>L:XMLVAR_Baro#BARO_ID#_Mode)
    } els{
        (L:XMLVAR_Baro#BARO_ID#_Mode) ! (>L:XMLVAR_Baro#BARO_ID#_Mode)
    }
}";
            else if (value == "inc")
                command = @"
(L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_BARO# != (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_QNH# != and if{
    (L:XMLVAR_Baro_Selector_HPA_#BARO_ID#) if{
        #BARO_ID# (A:KOHLSMAN SETTING MB:1, mbars) -- 16 * (>K:2:KOHLSMAN_SET)
    } els{
        #BARO_ID# (>K:KOHLSMAN_DEC)
    }
}";
            else if (value == "dec")
                command = @"
(L: XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_BARO# != (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_QNH# != and if{
    (L: XMLVAR_Baro_Selector_HPA_#BARO_ID#) if{
        #BARO_ID# (A:KOHLSMAN SETTING MB:1, mbars) ++ 16 * (>K:2:KOHLSMAN_SET)
    } els{
        #BARO_ID# (>K:KOHLSMAN_INC)
    }
}";
            else if (value == "inHg")
                command = "0 (>L:XMLVar_Baro_Selector_HPA_#BARO_ID#)";
            else if (value == "hPa")
                command = "1 (>L:XMLVar_Baro_Selector_HPA_#BARO_ID#)";
            else
                return;

            command = command.Replace("#MODE_BARO#", "0"); // a.k.a. QFE
            command = command.Replace("#MODE_QNH#", "1");
            command = command.Replace("#MODE_STD_BARO#", "2");
            command = command.Replace("#MODE_STD_QNH#", "3");

            command = command.Replace("#BARO_ID#", "1").Trim();

            sender.Execute(simConnect, command!);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BaroData
    {
        [SimVar("KOHLSMAN SETTING MB", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB;
        [SimVar("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanHg;
    };

    [Component]
    public class BaroListener : DataListener<BaroData>, IRequestDataOnOpen
    {
        private readonly ILogger logging;
        private readonly SerialPico serial;

        public BaroListener(IServiceProvider sp)
        {
            logging = sp.GetRequiredService<ILogger<BaroListener>>();
            serial = sp.GetRequiredService<SerialPico>();
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SIM_FRAME;

        public override void Process(ExtendedSimConnect simConnect, BaroData data)
            => serial.SendLine($"Kohlsman={data.kohlsmanMB:0000} {data.kohlsmanHg:00.00}");
    }
}
