﻿using Controlzmo.Hubs;
using Lombok.NET;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Controlzmo.Systems.Atc
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AtcAirlineData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        [SimVar("ATC AIRLINE", null, SIMCONNECT_DATATYPE.STRING64, 0.0f)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        [SimVar("ATC MODEL", null, SIMCONNECT_DATATYPE.STRING32, 0.0f)]
        public string model;
        // You *can* set this using one of these objects, but it won't affect the skin except at load time (possibly by forcing a "restart").
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        //[SimVar("ATC ID", null, SIMCONNECT_DATATYPE.STRING32, 0.0f)] // In 2024, this is randomly generated on load (discarding user input!).
        //public string tailNumber; // SDK says up to 10 characters! May not be a default for some liveries; for some it's junk.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [SimVar("TITLE", null, SIMCONNECT_DATATYPE.STRING128, 0.0f)]
        public string title; // This is what vAMSYS will use.
        // These two are MSFS2024 only.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [SimVar("LIVERY NAME", null, SIMCONNECT_DATATYPE.STRING128, 0.1f)]
        public string liveryName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        [SimVar("LIVERY FOLDER", null, SIMCONNECT_DATATYPE.STRING128, 0.1f)]
        public string liveryFolder;
    };

    [Component]
    public partial class AtcAirlineListener : DataListener<AtcAirlineData>, IOnAircraftLoaded, IOnSimStarted
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;
        private readonly bool isLocalSops;
        private readonly static Regex warmupRegex = new Regex(@"warm up(?: \((\d)m\))?", RegexOptions.IgnoreCase);
        private readonly static Regex cooldownRegex = new Regex(@"cool down(?: \((\d)m\))?", RegexOptions.IgnoreCase);
        private readonly static int DEFAULT_ENGINE_WAIT_MINUTES = 3;

        [Property]
        private int _warmupMinutes;
        [Property]
        private int _cooldownMinutes;

        public AtcAirlineListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
            isLocalSops = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioEdition"));
        }

        public void OnStarted(ExtendedSimConnect simConnect) => OnAircraftLoaded(simConnect);

        public void OnAircraftLoaded(ExtendedSimConnect simConnect) {
            hub.Clients.All.SetFromSim("atcAirline", "aircraft loaded");
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.ONCE);
        }

        public override async void Process(ExtendedSimConnect simConnect, AtcAirlineData data)
        {
            WarmupMinutes = CooldownMinutes = DEFAULT_ENGINE_WAIT_MINUTES;
            var icaoCode = Regex.Replace(data.model.ToUpper(), @"^ATCCOM\.AC_MODEL[ _](.*)\.0\.TEXT$", @"$1");
            var callsign = data.name.ToLower();
            var aircraftCfg = simConnect.AircraftFile.ToLower();
            // Some of the iniBuilds ones have the wrong ICAO code or even no model at all. :-(
            if (aircraftCfg.Contains("\\a21n\\")) icaoCode = "A21N";
            else if (aircraftCfg.Contains("\\inibuilds\\a330-300")) icaoCode = "A333";
            else if (aircraftCfg.Contains("\\inibuilds\\a330-200")) icaoCode = "A332";
            var sops = $"SOPs for '{icaoCode}' with callsign '{callsign}', file '{aircraftCfg}', title '{data.title}':";
            try
            {
                var doc = await loadXml();
                var context = new CustomContext {
                    { "callsign", callsign },
                    { "icaoType", icaoCode },
                    { "aircraft", aircraftCfg },
                    { "title", data.title },
                };
                var nodes = doc.DocumentElement?.SelectNodes(
                    @"//Airline[fn:matches($callsign, @callsign)]/Text[fn:matches($icaoType, @type) or fn:matches($aircraft, @aircraft) or fn:matches($title, @title)]", context);
                if (nodes?.Count == 0)
                    sops += $"\nNone available";
                else
                    foreach (var node in nodes!)
                        sops += $"\n\u2022 {(node as XmlElement)?.InnerText}";
                WarmupMinutes = Minutes(warmupRegex, sops);
                CooldownMinutes = Minutes(cooldownRegex, sops);
            }
            catch (Exception e)
            {
                sops = e.ToString();
            }
            await hub.Clients.All.SetFromSim("atcAirline", sops);

            //MSFS2020 is KittyHawk 11.0, 2024 is SunRise 12.0
            var extra = $"{simConnect.OpenData.szApplicationName} {simConnect.OpenData.dwApplicationVersionMajor}.{simConnect.OpenData.dwApplicationVersionMinor}";
            if (simConnect.OpenData.dwApplicationVersionMajor >= 12)
                extra += $"\nLivery name {data.liveryName}\n\tfolder {data.liveryFolder}";
            await hub.Clients.All.SetFromSim("fuelLog", extra);
        }

        private int Minutes(Regex regex, String sops)
        {
            var match = regex.Match(sops);
            return match.Success ? (match.Groups[1].Value == "" ? DEFAULT_ENGINE_WAIT_MINUTES: int.Parse(match.Groups[1].Value)) : 1;
        }

        private async Task<XmlDocument> loadXml()
        {
            var doc = new XmlDocument();
            await Task.Run(() => doc.Load(isLocalSops ? "SOPs.xml" : "https://github.com/gizmo71/FSInputMapper/raw/master/Controlzmo/SOPs.xml"));
            return doc;
        }
    }

    // From https://stackoverflow.com/questions/19498192/xpath-injection-mitigation/19704008#19704008
    class CustomContext : XsltContext
    {
        private Dictionary<string, object> m_values = new Dictionary<string, object>();

        public void Add(string name, object value) => m_values[name] = value;
        public override IXsltContextVariable ResolveVariable(string prefix, string name) => new XPathVariable(m_values[name]);
        public override int CompareDocument(string baseUri, string nextbaseUri) => throw new NotImplementedException();
        public override bool PreserveWhitespace(XPathNavigator node) => throw new NotImplementedException();

        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
        {
            if (prefix == "fn")
            {
                if (name == "matches")
                    return new XPathMatchesPolyfill();
            }
            throw new NotImplementedException();
        }

        public override bool Whitespace { get { throw new NotImplementedException(); } }

        private class XPathVariable : IXsltContextVariable
        {
            private object m_value;
            internal XPathVariable(object value) => m_value = value;
            public object Evaluate(XsltContext xsltContext) => m_value;
            public bool IsLocal { get { throw new NotImplementedException(); } }
            public bool IsParam { get { throw new NotImplementedException(); } }
            public XPathResultType VariableType { get { throw new NotImplementedException(); } }
        }

    }

    class XPathMatchesPolyfill : IXsltContextFunction // https://www.w3.org/TR/xpath-functions/#func-matches
    {
        public XPathResultType[] ArgTypes { get => new XPathResultType[] { XPathResultType.String, XPathResultType.String, XPathResultType.String }; }
        public int Maxargs { get => 3; }
        public int Minargs { get => 2; }
        public XPathResultType ReturnType { get => XPathResultType.Boolean; }

        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (args.Length == 3) throw new NotImplementedException("Regex flags not yet supported");
            var regex = StringFromNavigator(args[1]);
            var needle = new Regex(regex!);
            var haystack = StringFromNavigator(args[0]);
            return haystack == null ? false : needle.IsMatch(haystack);
        }

        private string? StringFromNavigator(object? stringOrNavigator)
        {
            if (stringOrNavigator == null) return null;
            if (stringOrNavigator is string s) return s;
            if (stringOrNavigator is XPathNodeIterator iterator)
            {
                // What an absolute dog's breakfast!
                iterator.MoveNext();
                iterator = iterator.Current!.SelectDescendants(XPathNodeType.Text, false);
                iterator.MoveNext();
                return iterator.Current!.Value;
            }
            throw new NotImplementedException($"Don't know how to handle {stringOrNavigator.GetType().FullName}");
        }
    }
}
