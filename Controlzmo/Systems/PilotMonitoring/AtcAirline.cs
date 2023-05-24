﻿using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Controlzmo.Systems.PilotMonitoring
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
    };

    [Component]
    public class AtcAirlineListener : DataListener<AtcAirlineData>, IOnSimStarted
    {
        private readonly IHubContext<ControlzmoHub, IControlzmoHub> hub;

        public AtcAirlineListener(IHubContext<ControlzmoHub, IControlzmoHub> hub)
        {
            this.hub = hub;
        }

        public void OnStarted(ExtendedSimConnect simConnect)
        {
            hub.Clients.All.SetFromSim("atcAirline", "Request sent...");
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);
        }

        public override void Process(ExtendedSimConnect simConnect, AtcAirlineData data)
        {
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.NEVER);

            var icaoCode = data.model.ToUpper();
            var callsign = data.name.ToLower();
            var sops = $"No SOPs available for '{icaoCode}' with callsign '{callsign}'";
            try
            {
                var doc = new XmlDocument();
//TODO: load async?
                doc.Load(true ? "D:\\MSFlightSimulator\\Development\\FSInputMapper\\Controlzmo\\SOPs.xml" : "https://github.com/gizmo71/FSInputMapper/raw/master/Controlzmo/SOPs.xml");
                var context = new CustomContext { { "callsign", callsign }, { "icaoType", icaoCode } };
                var nodes = doc.DocumentElement?.SelectNodes($"//Airline[fn:matches($callsign, @callsign)]/Text[fn:matches($icaoType, @type)]", context);
                if (nodes?.Count > 0)
                {
                    sops = "";
                    foreach (var node in nodes)
                    {
                        if (sops?.Length > 0) sops += '\n';
                        sops += (node as XmlElement)?.InnerText;
                    }
                }
            }
            catch (Exception e)
            {
                sops = e.ToString();
            }
            hub.Clients.All.SetFromSim("atcAirline", sops);
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
            var regex = stringFromNavigator(args[1]);
            var needle = new Regex(regex);
            var haystack = stringFromNavigator(args[0]);
            return haystack == null ? false : needle.IsMatch(haystack);
        }

        private string? stringFromNavigator(object? stringOrNavigator)
        {
            if (stringOrNavigator == null) return null;
            if (stringOrNavigator is string s) return s;
            if (stringOrNavigator is XPathNodeIterator iterator)
            {
                // What an absolute dog's breakfast!
                iterator.MoveNext();
                iterator = iterator.Current.SelectDescendants(XPathNodeType.Text, false);
                iterator.MoveNext();
                return iterator.Current.Value;
            }
            throw new NotImplementedException($"Don't know how to handle {stringOrNavigator.GetType().FullName}");
        }
    }
}
