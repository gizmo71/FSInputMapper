using System;
using Controlzmo.Hubs;
using SimConnectzmo;

namespace Controlzmo.Serial
{
    [Component]
    public class Pot : ISettable<Int16?>
    {
        public string GetId() => "pot";

        public void SetInSim(ExtendedSimConnect simConnect, Int16? value)
        {
            Console.Error.WriteLine($"Imaginary 'pot' set to {value}");
        }
    }
}
