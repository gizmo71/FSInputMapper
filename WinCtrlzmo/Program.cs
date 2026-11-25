using HidSharp;
using HidSharp.Reports;
using MobiFlightWwFcu;
using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Linq;

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
Console.Error.WriteLine($"***************** looked up {productId:X4}, found {displayControl.GetControllerName()}");
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
            wsServer.AddWebSocketService<SetStick>("/Stick/Set");
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

    internal class SetStick : WebSocketBehavior
    {
        private string _name;
        private HidDevice _device;
        private HidStream _stream;
        private const byte reportId = 0x02b;
private static byte item = 0;

        protected override void OnOpen() {
            var _productId = int.Parse(Context.QueryString["productId"]);
            if (!DeviceList.Local.TryGetHidDevice(out _device, 0x4098, _productId))
                throw new Exception("Failed to find stick");
Console.Error.WriteLine($"found {_device}");
            _name = Context.QueryString["led"];
foreach (var or in _device.GetReportDescriptor().OutputReports)
    Console.Error.WriteLine($"\t* O.R. {or.ReportID} {or.ReportType}");
/*foreach (var deviceItem in _device.GetReportDescriptor().DeviceItems)
    foreach (var report in deviceItem.Reports)
    {
        Console.WriteLine(string.Format("{0}: ReportID={1}, Length={2}, Items={3}",
                            report.ReportType, report.ReportID, report.Length, report.DataItems.Count));
        foreach (var dataItem in report.DataItems)
        {
            Console.WriteLine(string.Format("  {0} Elements x {1} Bits, Units: {2}, Expected Usage Type: {3}, Flags: {4}, Usages: {5}",
                dataItem.ElementCount, dataItem.ElementBits, dataItem.Unit.System, dataItem.ExpectedUsageType, dataItem.Flags,
                string.Join(", ", dataItem.Usages.GetAllValues().Select(usage => usage.ToString("X4") + " " + ((Usage)usage).ToString()))));
        }
    }
found Winwing WINWING URSA MINOR FIGHTER FLIGHT STICK R A09560721456623163E650B2 (VID 16536, PID 48170, version 0.99)
        * O.R. 2 Output
Input: ReportID=1, Length=30, Items=9
  1 Elements x 4 Bits, Units: EnglishRotation, Expected Usage Type: 0, Flags: Variable, NullState, Usages: 10039 GenericDesktopHatSwitch
  4 Elements x 1 Bits, Units: None, Expected Usage Type: 0, Flags: Constant, Variable, Usages:
  128 Elements x 1 Bits, Units: None, Expected Usage Type: PushButton, Flags: Variable, Usages: 90001 Button1, 90002 Button2, 90003 Button3, 90004 Button4, 90005 Button5, 90006 Button6, 90007 Button7, 90008 Button8, 90009 Button9, 9000A Button10, 9000B Button11, 9000C Button12, 9000D Button13, 9000E Button14, 9000F Button15, 90010 Button16, 90011 Button17, 90012 Button18, 90013 Button19, 90014 Button20, 90015 Button21, 90016 Button22, 90017 Button23, 90018 Button24, 90019 Button25, 9001A Button26, 9001B Button27, 9001C Button28, 9001D Button29, 9001E Button30, 9001F Button31, 90020 589856, 90021 589857, 90022 589858, 90023 589859, 90024 589860, 90025 589861, 90026 589862, 90027 589863, 90028 589864, 90029 589865, 9002A 589866, 9002B 589867, 9002C 589868, 9002D 589869, 9002E 589870, 9002F 589871, 90030 589872, 90031 589873, 90032 589874, 90033 589875, 90034 589876, 90035 589877, 90036 589878, 90037 589879, 90038 589880, 90039 589881, 9003A 589882, 9003B 589883, 9003C 589884, 9003D 589885, 9003E 589886, 9003F 589887, 90040 589888, 90041 589889, 90042 589890, 90043 589891, 90044 589892, 90045 589893, 90046 589894, 90047 589895, 90048 589896, 90049 589897, 9004A 589898, 9004B 589899, 9004C 589900, 9004D 589901, 9004E 589902, 9004F 589903, 90050 589904, 90051 589905, 90052 589906, 90053 589907, 90054 589908, 90055 589909, 90056 589910, 90057 589911, 90058 589912, 90059 589913, 9005A 589914, 9005B 589915, 9005C 589916, 9005D 589917, 9005E 589918, 9005F 589919, 90060 589920, 90061 589921, 90062 589922, 90063 589923, 90064 589924, 90065 589925, 90066 589926, 90067 589927, 90068 589928, 90069 589929, 9006A 589930, 9006B 589931, 9006C 589932, 9006D 589933, 9006E 589934, 9006F 589935, 90070 589936, 90071 589937, 90072 589938, 90073 589939, 90074 589940, 90075 589941, 90076 589942, 90077 589943, 90078 589944, 90079 589945, 9007A 589946, 9007B 589947, 9007C 589948, 9007D 589949, 9007E 589950, 9007F 589951, 90080 589952
  1 Elements x 16 Bits, Units: None, Expected Usage Type: 0, Flags: Variable, Usages: 10030 GenericDesktopX
  1 Elements x 16 Bits, Units: None, Expected Usage Type: 0, Flags: Variable, Usages: 10031 GenericDesktopY
  1 Elements x 16 Bits, Units: None, Expected Usage Type: 0, Flags: Variable, Usages: 10032 GenericDesktopZ
  1 Elements x 16 Bits, Units: None, Expected Usage Type: 0, Flags: Variable, Usages: 10033 GenericDesktopRx
  1 Elements x 16 Bits, Units: None, Expected Usage Type: 0, Flags: Variable, Usages: 10034 GenericDesktopRy
  1 Elements x 16 Bits, Units: None, Expected Usage Type: 0, Flags: Variable, Usages: 10036 GenericDesktopSlider
Input: ReportID=2, Length=30, Items=2
  13 Elements x 8 Bits, Units: None, Expected Usage Type: 0, Flags: Variable, Usages: FF0001 16711681
  128 Elements x 1 Bits, Units: None, Expected Usage Type: 0, Flags: Constant, Variable, Usages:
Output: ReportID=2, Length=14, Items=1
  13 Elements x 8 Bits, Units: None, Expected Usage Type: 0, Flags: Variable, Usages: FF0002 16711682*/

Console.Error.WriteLine($"\tMax output report size {_device.GetMaxOutputReportLength()}");
            _stream = _device.Open();
        }

        protected override void OnMessage(MessageEventArgs args) {
            var value = Byte.Parse(args.Data);
if (value != 0) ++item;
Console.Error.WriteLine($"Set stick {_name} to {args.Data} - TODO {item} = {value}");
            byte[] buffer = {
                reportId, 0x07, 0xBF, 0x00, 0x00, 0x03, 0x49, item, value, 0, 0, 0, 0, 0, 0
            };
            _stream.Write(buffer);
Console.Error.WriteLine("\twrite done");
            //_stream.Flush();
        }
        protected override void OnClose(CloseEventArgs _) {
            _stream?.Close();
        }
    }
}
