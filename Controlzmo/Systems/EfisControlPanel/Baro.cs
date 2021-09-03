using Controlzmo.Hubs;
using Controlzmo.Systems.JetBridge;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Runtime.InteropServices;

/*TODO: 1or0 (>A:KOHLSMAN SETTING STD) toggle mode (for pull/first push)?
What about QFE/QNH toggle on subsequent pushes?
Baro: `(>K:KOHLSMAN_INC/DEC)` works on inHg but in hPa needs multiple calls.
It seems that one cannot manually set 1014!
Only BARO_ID 1 actually works properly...

Random snippets found in MS and A32NX code; probably use this, not the above...:

**** A32NX_Interior_FCU.xml - BARO_ID is probably 0 left, 1 right, 2 standby
    <MODE_BARO>0</MODE_BARO> 0 QFE
    <MODE_QNH>1</MODE_QNH> 1 QNH
    <MODE_STD_BARO>2</MODE_STD_BARO> from QFE
    <MODE_STD_QNH>3</MODE_STD_QNH> from QNH
XMLVar_Baro_Selector_HPA_1 is 0 for inHg and 1 for hPa
    <ANTICLOCKWISE_CODE>
        (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_BARO# != (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_QNH# != and if{
            (L:XMLVAR_Baro_Selector_HPA_#BARO_ID#) if{
                #BARO_ID# (A:KOHLSMAN SETTING MB:1, mbars) -- 16 * (&gt;K:2:KOHLSMAN_SET)
            } els{
                #BARO_ID# (&gt;K:KOHLSMAN_DEC)
            }
        }
    </ANTICLOCKWISE_CODE>
    <CLOCKWISE_CODE>
        (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_BARO# != (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_QNH# != and if{
            (L:XMLVAR_Baro_Selector_HPA_#BARO_ID#) if{
                #BARO_ID# (A:KOHLSMAN SETTING MB:1, mbars) ++ 16 * (&gt;K:2:KOHLSMAN_SET)
            } els{
                #BARO_ID# (&gt;K:KOHLSMAN_INC)
            }
        }
    </CLOCKWISE_CODE>
    <PULL_CODE>
        (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_BARO# == if{
            #MODE_STD_BARO# (&gt;L:XMLVAR_Baro#BARO_ID#_Mode)
        } els{
            (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_QNH# == if{
                #MODE_STD_QNH# (&gt;L:XMLVAR_Baro#BARO_ID#_Mode)
            }
        }
    </PULL_CODE>
    <PUSH_CODE>
        (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_BARO# == if{
            #MODE_BARO# (&gt;L:XMLVAR_Baro#BARO_ID#_Mode)
        } els{
            (L:XMLVAR_Baro#BARO_ID#_Mode) #MODE_STD_QNH# == if{
                #MODE_QNH# (&gt;L:XMLVAR_Baro#BARO_ID#_Mode)
            } els{
                (L:XMLVAR_Baro#BARO_ID#_Mode) ! (&gt;L:XMLVAR_Baro#BARO_ID#_Mode)
            }
        }
    </PUSH_CODE>*/
namespace Controlzmo.Systems.EfisControlPanel
{
    [Component]
    public class BaroKnob : ISettable<string>
    {
        private readonly JetBridgeSender sender;

        public BaroKnob(IServiceProvider sp) => sender = sp.GetRequiredService<JetBridgeSender>();

        public string GetId() => "baroKnob";

        public void SetInSim(ExtendedSimConnect simConnect, string? value)
        {
            string? command = null;
            if (value == "pull")
                command = "(L:XMLVAR_Baro0_Mode) 0 == if{ 2 (>L:XMLVAR_Baro0_Mode) } els{ (L:XMLVAR_Baro0_Mode) 1 == if{ 3 (>L:XMLVAR_Baro0_Mode) } }";
            else if (value == "inc")
                command = "(L:XMLVAR_Baro1_Mode) 2 != (L:XMLVAR_Baro1_Mode) 3 != and if{ (L: XMLVAR_Baro_Selector_HPA_1) if{ 1 (A:KOHLSMAN SETTING MB:1, mbars) ++ 16 * (>K:2:KOHLSMAN_SET) } els{ 1 (>K:KOHLSMAN_INC) } }";
            else if (value == "inHg")
                command = "0 (>L:XMLVar_Baro_Selector_HPA_1)";
            else if (value == "hPa")
                command = "1 (>L:XMLVar_Baro_Selector_HPA_1)";
Console.Error.WriteLine(command);
            sender.Execute(simConnect, command!);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct BaroData
    {
        [SimVar("SEA LEVEL PRESSURE", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float seaLevelPressureMB;
        [SimVar("KOHLSMAN SETTING MB", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB;
        [SimVar("KOHLSMAN SETTING MB:2", "Millibars", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanMB2; // This is the ISIS
        [SimVar("KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT32, 0.1f)]
        public float kohlsmanHg;
    };

    [Component]
    public class BaroListener : DataListener<BaroData>, IRequestDataOnOpen
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly ILogger logging;

        public BaroListener(IHubContext<ControlzmoHub, IControlzmoHub> hub, ILogger<BaroListener> logger)
        {
            this.hub = hub;
            logging = logger;
        }

        public SIMCONNECT_PERIOD GetInitialRequestPeriod() => SIMCONNECT_PERIOD.SECOND;

        public override void Process(ExtendedSimConnect simConnect, BaroData data)
        {
            logging.LogDebug($"MSL {data.seaLevelPressureMB:0.0}MB\n"
                + $"Kohlsman {data.kohlsmanMB:0}MB (second {data.kohlsmanMB2:0}MB) {data.kohlsmanHg:0.##}Hg");
        }
    }
}
