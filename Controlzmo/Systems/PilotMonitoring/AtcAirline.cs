using Controlzmo.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectzmo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
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
            simConnect.RequestDataOnSimObject(this, SIMCONNECT_PERIOD.SECOND);
        }

        public override void Process(ExtendedSimConnect simConnect, AtcAirlineData data)
        {
            var icaoCode = data.model.ToUpper();
            var callsign = data.name.ToLower();
            var sops = $"No SOPs available for {icaoCode} with callsign {callsign}";
            try
            {
                var doc = new XmlDocument();
//TODO: load async?
                doc.Load("https://github.com/gizmo71/FSInputMapper/raw/master/Controlzmo/SOPs.xml");
                var context = new VariableContext { { "callsign", callsign }, { "icaoType", icaoCode } };
                var node = doc.DocumentElement?.SelectSingleNode($"//Airline[./Callsign/@of = $callsign]/Type[@icao=$icaoType]/Text", context);
                if (node != null)
                    sops = node.InnerText;
            }
            catch (Exception e)
            {
                sops = e.ToString();
            }
            hub.Clients.All.SetFromSim("atcAirline", sops);
        }
    }

    // From https://stackoverflow.com/questions/19498192/xpath-injection-mitigation/19704008#19704008
    class VariableContext : XsltContext
    {
        private Dictionary<string, object> m_values;

        public VariableContext()
        {
            m_values = new Dictionary<string, object>();
        }

        public void Add(string name, object value)
        {
            m_values[name] = value;
        }

        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            return new XPathVariable(m_values[name]);
        }

        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            throw new NotImplementedException();
        }

        public override bool PreserveWhitespace(XPathNavigator node)
        {
            throw new NotImplementedException();
        }

        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
        {
            throw new NotImplementedException();
        }

        public override bool Whitespace
        {
            get { throw new NotImplementedException(); }
        }

        private class XPathVariable : IXsltContextVariable
        {
            private object m_value;

            internal XPathVariable(object value)
            {
                m_value = value;
            }

            public object Evaluate(XsltContext xsltContext)
            {
                return m_value;
            }

            public bool IsLocal
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsParam
            {
                get { throw new NotImplementedException(); }
            }

            public XPathResultType VariableType
            {
                get { throw new NotImplementedException(); }
            }
        }

    }
}
