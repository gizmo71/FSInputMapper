using MobiFlightWwFcu;
using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WinCtrlzmo
{
    internal class Program
    {
        static internal WebSocketServer wsServer;
        static internal IDictionary<int, WinwingDisplayControl> displayControls = new Dictionary<int, WinwingDisplayControl>();

        static internal WinwingDisplayControl GetDisplayControl(int productId)
        {
            WinwingDisplayControl displayControl;
            lock (displayControls) {
                if (!displayControls.TryGetValue(productId, out displayControl)) {
                    try {
                        displayControl = new WinwingDisplayControl(productId, wsServer);
                        displayControls.Add(productId, displayControl);
Console.Error.WriteLine($"***************** looked up {productId}, found {displayControl.GetControllerName()}");
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
                        var backlight = "Backlight Percentage";
                        if (displayControl.GetLedNames().Contains(backlight))
                            displayControl.SetLed(backlight, 100);
                    }
                    catch (Exception ex) {
                        Console.Error.WriteLine("Failed to create and connect to {0}: {1}", productId, ex);
                    }
                }
            }
            return displayControl;
        }

        static void Main(string[] args)
        {
            wsServer = new WebSocketServer(System.Net.IPAddress.Loopback, 8666);
            wsServer.Start();
            wsServer.AddWebSocketService<SetLed>("/Led/Set");
            wsServer.AddWebSocketService<SetDisplay>("/Display/Set");
            Console.ReadKey(true);
        }
    }

    internal abstract class WinWingBehavior : WebSocketBehavior
    {
        protected WinwingDisplayControl _displayControl;
        protected override void OnOpen() => _displayControl = Program.GetDisplayControl(int.Parse(Context.QueryString["productId"]));
    }

    internal class SetLed : WinWingBehavior
    {
        private string _led;

        protected override void OnOpen() {
            base.OnOpen();
            _led = Context.QueryString["led"];
        }

        protected override void OnMessage(MessageEventArgs args)
        {
Console.Error.WriteLine($"Set LED {_led} to {args.Data}");
            _displayControl.SetLed(_led, Byte.Parse(args.Data));
        }
    }

    internal class SetDisplay : WinWingBehavior
    {
        private string _name;

        protected override void OnOpen() {
            base.OnOpen();
            _name = Context.QueryString["name"];
        }

        protected override void OnMessage(MessageEventArgs args) {
Console.Error.WriteLine($"Set display {_name} to {args.Data}");
            _displayControl.SetDisplay(_name, args.Data);
        }
    }
}
