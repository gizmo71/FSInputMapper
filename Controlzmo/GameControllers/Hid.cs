using HidSharp;
using System;
using System.ComponentModel;
using System.Threading;
//using Windows.Devices.Enumeration;

namespace Controlzmo.GameControllers
{
    [Component]
    public class Hid : KeepAliveWorker, CreateOnStartup
    {
        public Hid(IServiceProvider serviceProvider) : base(serviceProvider) { }

        //private DeviceWatcher? watcher;
        //private DeviceInformation? pac;

        protected override void OnStart(object? sender, DoWorkEventArgs args)
        {
            //watcher = DeviceInformation.CreateWatcher();
            //watcher.EnumerationCompleted += Wazzup;
            //watcher.Added += GotOne;
        }

        protected override void OnLoop(object? sender, DoWorkEventArgs args)
        {
            Thread.Sleep(6_666);
            //if (watcher!.Status == DeviceWatcherStatus.Created) watcher.Start();
            //var _ = Find();
            //_ = Find2();
            var hidDevice = DeviceList.Local.GetHidDeviceOrNull(0x4098, 0xB920)!;
            if (hidDevice != null)
            {
                Console.WriteLine($">>> got {hidDevice.DevicePath} = {hidDevice.GetProductName()}");
                foreach (var r in hidDevice.GetReportDescriptor().Reports) {
                    Console.WriteLine($"\t>>> report {r.ReportID} di {r.DeviceItem} dis {r.DataItems.Count}");
                    var stream = hidDevice.Open();
                    try
                    {
                        //stream.
                    } finally
                    {
                        stream.Close();
                    }
                }
            }
            else Console.WriteLine($">>> nope");
        }
/*
        // https://android.googlesource.com/kernel/common/+/bce1305c0ece3/drivers/hid/hid-lg-g15.c
        private const byte LG_G15_FEATURE_REPORT = 0x02;
        private const int LG_G15_TRANSFER_BUF_SIZE = 20;
        private async Task Find()
        {
            var deviceSelector = HidDevice.GetDeviceSelector(0x0001, 0x0004, 0x4098, 0xB920);
            var all = await DeviceInformation.FindAllAsync(deviceSelector);
            foreach (var device in all) {
                Console.Error.WriteLine($"\tdevice {device.Id}");
continue;
                var d = await HidDevice.FromIdAsync(device.Id, FileAccessMode.ReadWrite);
                try {
                    var fr = d.CreateFeatureReport(LG_G15_FEATURE_REPORT);
                    var buffer = new byte[LG_G15_TRANSFER_BUF_SIZE];
                    buffer[0] = LG_G15_FEATURE_REPORT;
                    buffer[1] = 1; // LED number indexed from 1
                    buffer[2] = (byte) new Random().Next(16); // brightness << (g15_led->led * 4);
                    buffer[3] = 0;
                    fr.Data = buffer.AsBuffer();
// ret = hid_hw_raw_request(g15->hdev, LG_G15_FEATURE_REPORT, g15->transfer_buf, 4, HID_FEATURE_REPORT, HID_REQ_SET_REPORT);
                    var result = d.SendFeatureReportAsync(fr);
Console.Error.WriteLine($"wrote and got {result.Status}");
                } catch (Exception e) {
                    Console.Error.WriteLine($"it went boom :-( {e.StackTrace}");
                } finally {
                    d?.Dispose();
                };
            }
Console.Error.WriteLine($"\tfound all");
        }
        private async Task Find2()
        {
Console.Error.WriteLine($"\t------ hello HID? {pac?.Id}");
            if (pac != null) {
                var d = await HidDevice.FromIdAsync(pac.Id, FileAccessMode.ReadWrite);
Console.Error.WriteLine($"\t----------- did it? {d}");
                var cds = d?.GetNumericControlDescriptions(HidReportType.Input, 0, 0);
                if (cds != null)
                    foreach (var cd in cds)
                        Console.Error.WriteLine($"\t\t---- {cd.Id}");
Console.Error.WriteLine($"\t--^^-- cds {cds}");
                d?.Dispose();
            }
        }

        private void GotOne(DeviceWatcher sender, DeviceInformation args)
        {
//HID got Windows.Devices.Enumeration.DeviceInformation id \\?\HID#VID_4098&PID_B920#7&3639d869&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030} 'WINWING URSA MINOR 32 Throttle Metal L' DeviceInterface
//HID got Windows.Devices.Enumeration.DeviceInformation id \\?\USB#VID_4098&PID_B920#16E2F069CE5348C143367042#{a5dcbf10-6530-11d2-901f-00c04fb951ed} 'WINWING URSA MINOR 32 Throttle Metal L' DeviceInterface
            if (args.Id.ToUpper().Contains("VID_4098") && !args.Id.ToUpper().Contains("\\USB#"))
            {
               Console.Error.WriteLine($"  HID got {args} id {args.Id} '{args.Name}' {args.Kind}");
               pac = args;
            }
        }

        private void Wazzup(DeviceWatcher sender, object args)
        {
            Console.Error.WriteLine("HID enumeration complete");
            watcher!.Stop();
        }
*/
        protected override void OnStop(object? sender, DoWorkEventArgs args)
        {
            //watcher?.Stop();
            //watcher = null;
        }
    }
}
