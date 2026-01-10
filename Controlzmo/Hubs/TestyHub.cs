using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Controlzmo.Hubs
{
    [Component]
    public partial class Ticker : CreateOnStartup {
        private int count = 0;
        private readonly IHubContext<TestyHub> hub;

        public Ticker(IServiceProvider sp) {
            hub = sp.GetRequiredService<IHubContext<TestyHub>>();
            var timer = new Timer(1000);
            timer.Elapsed += Tick;
            timer.Start();
        }

        private void Tick(object? sender, ElapsedEventArgs args) {
            hub.Clients.All.SendAsync("ToBrowser", count++, "foo");
        }
    }

    [RequiredArgsConstructor]
    public partial class TestyHub : Hub
    {
        public async Task FromBrowser(string a, int b)
        {
Console.Error.WriteLine($"Got message from browser with {a} and {b}");
            await Task.CompletedTask;
        }
    }
}
