using MobiFlightWwFcu;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WinCtrlzmo
{
    internal class Program
    {
        static internal WinwingDisplayControl displayControl;

        static void Main(string[] args)
        {
            var wsServer = new WebSocketServer(System.Net.IPAddress.Loopback, 8666);
            wsServer.Start();
            wsServer.AddWebSocketService<SetLed>("/Led/Set");
            wsServer.AddWebSocketService<SetDisplay>("/Display/Set");
            try {
                displayControl = new WinwingDisplayControl(0xB920, wsServer);
Console.Error.WriteLine("***************** displays");
                foreach (var name in displayControl.GetDisplayNames())
                    Console.Error.WriteLine($"\t{name}");
Console.Error.WriteLine("***************** LEDs");
// "Trim Value", "Trim Dashes On/Off", "LCD Test On/Off"
// "FAULT_1", "FIRE_1", "FAULT_2", "FIRE_2", "Vibration 1 Percentage", "Vibration 2 Percentage", "Backlight Percentage", "LED Percentage", "LCD Percentage"
                foreach (var name in displayControl.GetLedNames())
                    Console.Error.WriteLine($"\t{name}");
Console.Error.WriteLine("***************** Services");
                foreach (var path in wsServer.WebSocketServices.Paths)
                    Console.Error.WriteLine($"\t{path}"); // Shows nothing registered for this device
Console.Error.WriteLine("***************** Attempting to set stuff");
                displayControl.Connect();
                /*displayControl.SetDisplay("Trim Value", "+12.3");
                displayControl.SetLed("FAULT_1", 0);
                displayControl.SetLed("FAULT_2", 0);
                displayControl.SetLed("FIRE_1", 0);
                displayControl.SetLed("FIRE_2", 0);
                displayControl.SetLed("LED Percentage", 100);
                displayControl.SetLed("LCD Percentage", 100);*/
                displayControl.SetLed("Backlight Percentage", 100);
            }
            finally
            {
                Console.ReadKey(true);
                wsServer.Stop();
            }
        }
    }

    internal class SetLed : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs args)
        {
            Console.Error.WriteLine($"Do LED {args.Data}");
            //Program.displayControl.SetLed("FIRE_1", 0);
        }
    }

    internal class SetDisplay : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs args)
        {
            Program.displayControl.SetDisplay("Trim Value", args.Data);
        }
    }
}
