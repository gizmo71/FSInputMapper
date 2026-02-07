using MobiFlightWwFcu;
using System;
using System.Xml.Linq;
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
        private string _led;
        protected override void OnOpen() => _led = Context.QueryString["led"];
        protected override void OnMessage(MessageEventArgs args)
        {
Console.Error.WriteLine($"Set LED {_led} to {args.Data}");
            Program.displayControl.SetLed(_led, Byte.Parse(args.Data));
        }
    }

    internal class SetDisplay : WebSocketBehavior
    {
        private string _name;
        protected override void OnOpen() => _name = Context.QueryString["name"];
        protected override void OnMessage(MessageEventArgs args) {
Console.Error.WriteLine($"Set display {_name} to {args.Data}");
            Program.displayControl.SetDisplay(_name, args.Data);
        }
    }
}
