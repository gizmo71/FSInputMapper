using Controlzmo.Systems.PilotMonitoring;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Device.Location;

namespace Controlzmo.Systems.Atc
{
    public class OfpWaypoint
    {
        public readonly string Id;
        public readonly GeoCoordinate position;
        public readonly Double fuelUsed;
        public readonly Double minFOB;
        public readonly Double planFOB;

        public OfpWaypoint(XmlElement node)
        {
            Id = node.SelectSingleNode("./name")?.InnerText ?? "?";
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

        public override string ToString() => $"{Id}@{position}";
    }

    [Component]
    public partial class OperationalFlightPlan : IOnGroundHandler
    {
        private static readonly XmlDocument EMPTY = new XmlDocument();
        private XmlDocument ofp = EMPTY;

        public void OnGroundHandler(ExtendedSimConnect simConnect, bool isOnGround)
        {
            if (isOnGround)
                ofp = EMPTY;
            else
                Task.Run(Load);
        }

        private void Load()
        {
            ofp = new XmlDocument();
            ofp.Load("https://www.simbrief.com/api/xml.fetcher.php?username=gizmo71");
        }

        public Boolean IsValid { get => ofp != EMPTY; }

        public List<OfpWaypoint> GetFixes()
        {
            var nodes = ofp.DocumentElement?.SelectNodes(@"//navlog/fix");
            var fixes = new List<OfpWaypoint>(nodes?.Count ?? 0);
            foreach (var node in nodes!)
try {
                fixes.Add(new OfpWaypoint((XmlElement) node));
} catch (Exception ex) { /*Console.Error.WriteLine($"***---*** Bugger {ex}");*/ }
            return fixes;
        }
    }
}
