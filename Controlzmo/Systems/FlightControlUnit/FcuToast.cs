using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;

namespace Controlzmo.Systems.FlightControlUnit
{
    [Component, RequiredArgsConstructor]
    public partial class FcuToast
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        private string _left = "";
        private string _right = "";

        internal string Left { set => update(value, ref _left); }
        internal string Right { set => update(value, ref _right); }

        private void update(string newValue, ref string field)
        {
            if (newValue != field)
            {
                field = newValue;
                hub.Clients.All.Toast("FCU", field);
            }
        }
    }
}
