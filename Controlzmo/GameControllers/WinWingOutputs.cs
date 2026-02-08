using Lombok.NET;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel.Syndication;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Controlzmo.GameControllers
{
    [Component]
    public class UrsaMinorOutputs
    {
        private readonly IDictionary<string, WebSocket> clients = new Dictionary<string, WebSocket>();
        private readonly UrlEncoder urlEscaper = UrlEncoder.Default;

        private void SendTo(string pathAndQueryString, string request)
        {
            WebSocket? webSocket;
            lock (clients)
            {
                if (!clients.TryGetValue(pathAndQueryString, out webSocket)) {
                    webSocket = new WebSocket($"ws://localhost:8666{pathAndQueryString}");
                    clients.Add(pathAndQueryString, webSocket);
                }
            }
            Task.Run(() =>
            {
                if (!webSocket!.IsAlive)
                    webSocket.Connect();
                webSocket.Send(request);
            });
        }

        internal void Send(ushort productId, string path, string name, string value, string body)
        {
            try {
                var escapedValue = urlEscaper.Encode(value);
                SendTo($"/{path}/Set?{name}={escapedValue}&productId={productId}", body);
            } catch (Exception ex) {
                Console.Error.WriteLine("Failed to send {1} to {2}: {0}", ex, name, path);
            }
        }

        internal void SendDisplay(string name, string value) => Send(UrsaMinorThrottle.PRODUCT_ID, "Display", "name", name, value);
        internal void SendLed(string name, string value) => Send(UrsaMinorThrottle.PRODUCT_ID, "Led", "led", name, value);

        public void SetTrimDisplay(int decaUnits) {
            SendDisplay("Trim Value", $"{decaUnits / 10.0:+00.0;-00.0}");
            SendDisplay("Trim Dashes On/Off", "0");
            SendDisplay("LCD Test On/Off", "0");
        }
// "Trim Dashes On/Off" (setting this to >0 produces three dashes; can't unset directly)
// "LCD Test On/Off" (setting to >0 produces all the segments lit; again, unset by sending a different name)

        public void SetEngineWarning(int engine, bool isFire, bool isOn) => SendLed($"{(isFire ? "FIRE" : "FAULT")}_{engine}", isOn ? "1" : "0");

        public void SetThrottleVibrations(byte percent)
        {
            for (int i = 1; i <= 2; ++i)
                SendLed($"Vibration {i} Percentage", percent.ToString());
        }
//TODO: Other LEDs: "Backlight Percentage", "LED Percentage" (no idea what it does!), "LCD Percentage"
    }

    [Component, RequiredArgsConstructor]
    public partial class TestDaOutputs : IAxisCallback<T16000mHotas>
    {
        private readonly UrsaMinorOutputs output;

        public int GetAxis() => T16000mHotas.AXIS_WHEEL;

        public void OnChange(ExtendedSimConnect _, double old, double @new)
        {
            //output.SendDisplay("LCD Test On/Off", @new > 0.5 ? "1" : "");
            //output.SetVibrations((byte) percent);
//TODO: does the stick really not have vibration or backlight supported?
            output.Send(UrsaMinorFighterR.PRODUCT_ID, "Led", "led", "bar", "body");
        }
    }
}
