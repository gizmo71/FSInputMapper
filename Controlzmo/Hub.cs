using Microsoft.AspNetCore.SignalR;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Controlzmo
{
    public class LightHub : Hub
    {
        public async Task TestMessage(string message)
        {
            await Clients.All.SendAsync(MethodName(), message);
        }

        protected string MethodName([CallerMemberName] string name = "unknown")
        {
            return name;
        }
    }
}
