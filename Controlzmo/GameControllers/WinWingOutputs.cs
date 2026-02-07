using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Controlzmo.GameControllers
{
    [Component]
    public class UrsaMinorOutputs
    {
        private IDictionary<string, WebSocket> clients = new Dictionary<string, WebSocket>();

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

        public void SetTrimDisplay(int decaUnits)
        {
            try {
                var encoded = $"{decaUnits / 10.0:+00.0;-00.0}";
                SendTo("/Display/Set?name=Trim%20Value", encoded);
            } catch (Exception ex) {
                Console.Error.WriteLine("Failed to send trim display: {0}", ex);
            }
        }

        public void SetEngineWarning(int engine, bool isFire, bool isOn)
        {
            var prefix = isFire ? "FIRE" : "FAULT";
            SendTo($"/Led/Set?led={prefix}_{engine}", isOn ? "1" : "0");
        }
    }
}
