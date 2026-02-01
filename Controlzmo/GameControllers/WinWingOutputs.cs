using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Controlzmo.GameControllers
{
    [Component]
    public class UrsaMinorOutputs : KeepAliveWorker
    {
        private WebSocket? client;

        public UrsaMinorOutputs(IServiceProvider sp) : base(sp) { }

        protected override void OnStart(object? sender, DoWorkEventArgs args)
        {
            client = new WebSocket("ws://localhost:8666/Display/Set");
        }

        protected override void OnLoop(object? sender, DoWorkEventArgs args)
        {
            if (!client!.IsAlive)
                client.Connect();
        }

        protected override void OnStop(object? sender, DoWorkEventArgs args)
        {
            client?.Close();
        }

        public void SetTrimDisplay(int decaUnits)
        {
            try {
                var encoded = $"{decaUnits / 10.0:+00.0;-00.0}";
                Task.Run(() => client?.Send(encoded));
            } catch (Exception ex) {
                Console.Error.WriteLine("Failed to send trim display: {0}", ex);
            }
        }
    }
}
