using System;
using Microsoft.Extensions.DependencyInjection;

namespace FSInputMapper
{

    public static class FSIMTrigger
    {
        public const string SPD_MAN = "Managed Speed";
        public const string SPD_SEL = "Selected Speed";
        public const string SPD_1_FASTER = "Speed +1";
        public const string SPD_10_FASTER = "Speed +10";
        public const string SPD_1_SLOWER = "Speed -1";
        public const string SPD_10_SLOWER = "Speed -10";
        public const string HDG_MAN = "Managed Heading";
        public const string HDG_SEL = "Selected Heading";
        public const string HDG_RIGHT_1 = "Heading +1";
        public const string HDG_RIGHT_10 = "Heading +10";
        public const string HDG_LEFT_1 = "Heading -1";
        public const string HDG_LEFT_10 = "Heading -10";
        public const string ALT_MAN = "Managed Altitude";
        public const string ALT_SEL = "Selected Altitude";
        public const string ALT_UP_100 = "Altitude +100";
        public const string ALT_UP_1000 = "Altitude +1000";
        public const string ALT_DOWN_100 = "Altitude -100";
        public const string ALT_DOWN_1000 = "Altitude -1000";
        public const string VS_STOP = "Level Off";
        public const string VS_SEL = "VS Engage";
        public const string VS_UP = "VS Up";
        public const string VS_DOWN = "VS Down";
        public const string TOGGLE_LOC_MODE = "Toggle Loc";
        public const string TOGGLE_APPR_MODE = "Toggle Appr";
    }

    public class FSIMTriggerArgs : EventArgs
    {
        public string? What { get; set; }
    }

    [Singleton]
    public class FSIMTriggerBus
    {
//TODO: only need this until the 'triggers' are injected by the things using them
private readonly IServiceProvider sp;
public FSIMTriggerBus(IServiceProvider sp) { this.sp = sp; }
        public event EventHandler<FSIMTriggerArgs>? OnTrigger;
        public void Trigger(object sender, string what)
        {
sp.GetService<FSIMTriggerHandler>();
            OnTrigger?.Invoke(sender, new FSIMTriggerArgs() { What = what });
        }
    }

}
