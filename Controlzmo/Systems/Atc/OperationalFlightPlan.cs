using Controlzmo.Systems.PilotMonitoring;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Device.Location;
using Lombok.NET;
using Controlzmo.Systems.JetBridge;

namespace Controlzmo.Systems.Atc
{
    public class OfpWaypoint
    {
        public readonly string Name;
        public readonly string Ident;
        public readonly GeoCoordinate position;
        public readonly Double fuelUsed;
        public readonly Double minFOB;
        public readonly Double planFOB;

        internal OfpWaypoint(XmlElement node)
        {
            Name = node.SelectSingleNode("./name")?.InnerText ?? "?";
            Ident = node.SelectSingleNode("./ident")?.InnerText ?? "?";
            var latitude = NodeToNumber(node, "pos_lat");
            var longitude = NodeToNumber(node, "pos_long");
            position = new GeoCoordinate(latitude, longitude);
            fuelUsed = NodeToNumber(node, "fuel_totalused");
            minFOB = NodeToNumber(node, "fuel_min_onboard");
            planFOB = NodeToNumber(node, "fuel_plan_onboard");
        }

        private Double NodeToNumber(XmlNode node, String name)
        {
            var asText = node.SelectSingleNode($"./{name}")?.InnerText;
            if (asText == null) throw new Exception($"no {name}");
            return Double.Parse(asText!);
        }

        public override string ToString() => $"{Ident}@{position}";
    }

    [Component, RequiredArgsConstructor]
    public partial class OperationalFlightPlan : IOnGroundHandler
    {
        private static readonly XmlDocument EMPTY = new XmlDocument();
        private XmlDocument ofp = EMPTY;
        private readonly JetBridgeSender sender;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            if (isOnGround)
                ofp = EMPTY;
            else
                Task.Run(Load).ContinueWith((ofpReadTask) => { ofp = ofpReadTask.Result;});
        }

        public void ReadVSpeeds(ExtendedSimConnect simConnect)
        {
            Task.Run(Load).ContinueWith((ofpReadTask) => AdvertiseVSpeeds(ofpReadTask.Result, simConnect));
        }

        private XmlDocument Load()
        {
            var newOfp = new XmlDocument();
            newOfp.Load("https://www.simbrief.com/api/xml.fetcher.php?username=gizmo71");
            return newOfp;
        }

        public Boolean IsValid { get => ofp != EMPTY; }

        public List<OfpWaypoint> GetFixes()
        {
            var nodes = ofp.DocumentElement?.SelectNodes(@"//navlog/fix");
            var fixes = new List<OfpWaypoint>(nodes?.Count ?? 0);
            foreach (var node in nodes!)
try {
                fixes.Add(new OfpWaypoint((XmlElement) node));
} catch (Exception ex) { Console.Error.WriteLine($"***---*** Bugger {ex}"); }
            return fixes;
        }

        private void AdvertiseVSpeeds(XmlDocument tempOfp, ExtendedSimConnect simConnect)
        {
            var runway = tempOfp.SelectSingleNode(@"//tlr/takeoff/runway[identifier = ../conditions/planned_runway]");
            foreach(var (nodeName, varName) in new Dictionary<string, string>
                {
                    ["speeds_v1"] = "L:AIRLINER_V1_SPEED",
                    ["speeds_vr"] = "L:AIRLINER_VR_SPEED",
                    ["speeds_v2"] = "L:AIRLINER_V2_SPEED",
                })
            {
                var speed = runway?.SelectSingleNode($"./{nodeName}/text()");
                sender.Execute(simConnect, $"({varName}) 0 == if{{ {speed?.Value ?? "0"} (>{varName}) }}");
            }
        }
    }
}
